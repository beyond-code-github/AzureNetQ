namespace AzureNetQ.Producer
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Default implementation of AzureNetQ's request-response pattern
    /// </summary>
    public class Rpc : IRpc
    {
        private readonly IConventions conventions;

        private readonly IAzureAdvancedBus advancedBus;

        private readonly IConnectionConfiguration configuration;

        private readonly ConcurrentDictionary<RpcKey, string> responseQueues = new ConcurrentDictionary<RpcKey, string>();

        private readonly ConcurrentDictionary<string, ResponseAction> responseActions = new ConcurrentDictionary<string, ResponseAction>();

        private readonly TimeSpan disablePeriodicSignaling = TimeSpan.FromMilliseconds(-1);

        private const string ReplyToKey = "ReplyTo";

        private const string IsFaultedKey = "IsFaulted";

        private const string ExceptionMessageKey = "ExceptionMessage";

        public Rpc(IConventions conventions, IAzureAdvancedBus advancedBus, IConnectionConfiguration configuration)
        {
            Preconditions.CheckNotNull(conventions, "conventions");
            Preconditions.CheckNotNull(advancedBus, "advancedBus");
            Preconditions.CheckNotNull(configuration, "configuration");

            this.conventions = conventions;
            this.advancedBus = advancedBus;
            this.configuration = configuration;
        }

        public Task<TResponse> Request<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(request, "request");

            var correlationId = Guid.NewGuid();
            var tcs = new TaskCompletionSource<TResponse>();

            var timer = new Timer(state =>
            {
                ((Timer)state).Dispose();
                tcs.TrySetException(new TimeoutException(
                    string.Format("Request timed out. CorrelationId: {0}", correlationId.ToString())));
            });

            timer.Change(TimeSpan.FromSeconds(configuration.Timeout), disablePeriodicSignaling);

            responseActions.TryAdd(
                correlationId.ToString(),
                new ResponseAction
                    {
                        OnSuccess = message =>
                            {
                                timer.Dispose();
                                var isFaulted = false;
                                var exceptionMessage = "The exception message has not been specified.";

                                if (message.Properties.ContainsKey(IsFaultedKey))
                                {
                                    isFaulted = Convert.ToBoolean(message.Properties[IsFaultedKey]);
                                }

                                if (message.Properties.ContainsKey(ExceptionMessageKey))
                                {
                                    exceptionMessage = message.Properties[ExceptionMessageKey].ToString();
                                }
                                
                                if (isFaulted)
                                {
                                    tcs.TrySetException(new AzureNetQResponderException(exceptionMessage));
                                }
                                else
                                {
                                    tcs.TrySetResult(message.GetBody<TResponse>());
                                }
                            }
                    });

            var queueName = SubscribeToResponse<TRequest, TResponse>();
            RequestPublish(request, queueName, correlationId);

            return tcs.Task;
        }

        private string SubscribeToResponse<TRequest, TResponse>()
            where TResponse : class
        {
            var rpcKey = new RpcKey { Request = typeof(TRequest), Response = typeof(TResponse) };

            responseQueues.AddOrUpdate(rpcKey,
                key =>
                {
                    var name = conventions.RpcReturnQueueNamingConvention();
                    var queue = advancedBus.QueueDeclare(name, autoDelete: true);
                    queue.OnMessageAsync(message => Task.Factory.StartNew(() =>
                    {
                        ResponseAction responseAction;
                        if (responseActions.TryRemove(message.CorrelationId, out responseAction))
                        {
                            responseAction.OnSuccess(message);
                        }
                    }), new OnMessageOptions { AutoComplete = true, MaxConcurrentCalls = configuration.MaxConcurrentCalls });

                    return name;
                },
                (_, queueName) => queueName);

            return responseQueues[rpcKey];
        }

        private void RequestPublish<TRequest>(TRequest request, string returnQueueName, Guid correlationId) where TRequest : class
        {
            var requestMessage = new BrokeredMessage(request)
                                     {
                                         CorrelationId = correlationId.ToString(),
                                         TimeToLive = TimeSpan.FromMilliseconds(configuration.Timeout * 1000)
                                     };

            requestMessage.Properties.Add(ReplyToKey, returnQueueName);

            var routingKey = conventions.QueueNamingConvention(typeof(TRequest));
            var queue = this.advancedBus.QueueDeclare(routingKey);

            queue.SendAsync(requestMessage);
        }

        public IDisposable Respond<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(responder, "responder");

            var routingKey = conventions.RpcRoutingKeyNamingConvention(typeof(TRequest));
            var queue = advancedBus.QueueDeclare(routingKey);

            queue.OnMessageAsync(
                requestMessage =>
                {
                    var tcs = new TaskCompletionSource<object>();

                    responder(requestMessage.GetBody<TRequest>()).ContinueWith(
                        task =>
                        {
                            var responseQueue =
                                advancedBus.QueueDeclare(requestMessage.Properties[ReplyToKey] as string);
                            if (task.IsFaulted)
                            {
                                if (task.Exception != null)
                                {
                                    var body = Activator.CreateInstance<TResponse>();
                                    var responseMessage = new BrokeredMessage(body);
                                    responseMessage.Properties.Add(IsFaultedKey, true);
                                    responseMessage.Properties.Add(
                                        ExceptionMessageKey,
                                        task.Exception.InnerException.Message);
                                    responseMessage.CorrelationId = requestMessage.CorrelationId;
                                    responseQueue.SendAsync(responseMessage);

                                    // Set the result to null and don't indicate fault otherwise this message will be retried by service bus
                                    tcs.SetResult(null);
                                }
                            }
                            else
                            {
                                var responseMessage = new BrokeredMessage(task.Result) { CorrelationId = requestMessage.CorrelationId };
                                responseQueue.SendAsync(responseMessage);
                                tcs.SetResult(null);
                            }
                        });

                    return tcs.Task;
                },
                new OnMessageOptions { AutoComplete = true, MaxConcurrentCalls = configuration.MaxConcurrentCalls });

            return null;
        }
    }
}