namespace AzureNetQ.Producer
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using AzureNetQ.FluentConfiguration;

    using Microsoft.ServiceBus.Messaging;

    using Newtonsoft.Json;

    /// <summary>
    /// Default implementation of AzureNetQ's request-response pattern
    /// </summary>
    public class Rpc : IRpc
    {
        private const string ReplyToKey = "ReplyTo";

        private const string IsFaultedKey = "IsFaulted";

        private const string ExceptionMessageKey = "ExceptionMessage";

        private const string SerializedExceptionKey = "SerializedException";

        private readonly IConventions conventions;

        private readonly IAzureAdvancedBus advancedBus;

        private readonly IConnectionConfiguration connectionConfiguration;

        private readonly ISerializer serializer;

        private readonly IAzureNetQLogger logger;

        private readonly IExceptionHandler exceptionHandler;

        private readonly ConcurrentDictionary<RpcKey, string> responseQueues = new ConcurrentDictionary<RpcKey, string>();

        private readonly ConcurrentDictionary<string, ResponseAction> responseActions = new ConcurrentDictionary<string, ResponseAction>();

        private readonly TimeSpan disablePeriodicSignaling = TimeSpan.FromMilliseconds(-1);

        public Rpc(
            IConventions conventions,
            IAzureAdvancedBus advancedBus,
            IConnectionConfiguration connectionConfiguration,
            ISerializer serializer,
            IAzureNetQLogger logger)
        {
            Preconditions.CheckNotNull(conventions, "conventions");
            Preconditions.CheckNotNull(advancedBus, "advancedBus");
            Preconditions.CheckNotNull(connectionConfiguration, "configuration");
            Preconditions.CheckNotNull(serializer, "serializer");
            Preconditions.CheckNotNull(logger, "logger");

            this.conventions = conventions;
            this.advancedBus = advancedBus;
            this.connectionConfiguration = connectionConfiguration;
            this.serializer = serializer;
            this.logger = logger;
            this.exceptionHandler = new ExceptionHandler(this.logger);
        }

        public Task<TResponse> Request<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            return this.Request<TRequest, TResponse>(request, x => { });
        }

        public Task<TResponse> Request<TRequest, TResponse>(TRequest request, Action<IRequestConfiguration> configure)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(request, "request");

            var responderConfiguration = new RequestConfiguration();
            configure(responderConfiguration);

            var correlationId = Guid.NewGuid();
            var tcs = new TaskCompletionSource<TResponse>();

            var timer = new Timer(state =>
            {
                ((Timer)state).Dispose();
                tcs.TrySetException(new TimeoutException(
                    string.Format("Request timed out. CorrelationId: {0}", correlationId.ToString())));
            });

            timer.Change(TimeSpan.FromSeconds(this.connectionConfiguration.Timeout), this.disablePeriodicSignaling);

            this.responseActions.TryAdd(
                correlationId.ToString(),
                new ResponseAction
                {
                    OnSuccess = message =>
                    {
                        timer.Dispose();
                        var isFaulted = false;
                        var exceptionMessage = "The exception message has not been specified.";
                        Exception ex = null;

                        if (message.Properties.ContainsKey(IsFaultedKey))
                        {
                            isFaulted = Convert.ToBoolean(message.Properties[IsFaultedKey]);
                        }

                        if (message.Properties.ContainsKey(ExceptionMessageKey))
                        {
                            exceptionMessage = message.Properties[ExceptionMessageKey].ToString();
                        }

                        if (message.Properties.ContainsKey(SerializedExceptionKey))
                        {
                            ex = JsonConvert.DeserializeObject<Exception>(message.Properties[SerializedExceptionKey].ToString());
                        }

                        if (isFaulted)
                        {
                            tcs.TrySetException(new AzureNetQResponderException(exceptionMessage, ex));
                        }
                        else
                        {
                            var content = message.GetBody<string>();
                            var response = this.serializer.StringToMessage<TResponse>(content);
                            tcs.TrySetResult(response);
                        }
                    }
                });

            var queueName = this.SubscribeToResponse<TRequest, TResponse>();
            RequestPublish(request, queueName, correlationId, responderConfiguration);

            return tcs.Task;
        }

        public IDisposable Respond<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : class
            where TResponse : class
        {
            return this.Respond(responder, x => { });
        }

        public IDisposable Respond<TRequest, TResponse>(
            Func<TRequest, Task<TResponse>> responder,
            Action<IRespondConfiguration> configure)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(responder, "responder");

            var responderConfiguration = new RespondConfiguration();
            configure(responderConfiguration);

            var routingKey = this.conventions.RpcRoutingKeyNamingConvention(typeof(TRequest));
            var queue = this.advancedBus.QueueDeclare(routingKey);

            var messageTracker = new ConcurrentDictionary<string, int>();
            var onMessageOptions = new OnMessageOptions
                                       {
                                           AutoComplete = false,
                                           MaxConcurrentCalls =
                                               this.connectionConfiguration.MaxConcurrentCalls
                                       };

            onMessageOptions.ExceptionReceived += this.exceptionHandler.ExceptionReceived;

            queue.OnMessageAsync(
                requestMessage =>
                {
                    this.InfoWrite(
                        routingKey,
                        requestMessage.MessageId,
                        string.Format("Received request with correlation id: {0}", requestMessage.CorrelationId));

                    int messageAffinity;
                    if (requestMessage.ShouldBeFilteredByAffinity(
                        responderConfiguration.AffinityResolver,
                        out messageAffinity))
                    {
                        if (!responderConfiguration.AffinityResolver(messageAffinity))
                        {
                            var seenCount = TrackMessageSeenCount(messageTracker, requestMessage);
                            var affinityCycle = GetAffinityCycle(requestMessage);
                            var nextAffinityCycle = affinityCycle + 1;

                            if (seenCount > 1)
                            {
                                this.InfoWrite(
                                    routingKey,
                                    requestMessage.MessageId,
                                    string.Format("Consumer has already seen this request"));

                                ResetMessageSeenCount(messageTracker, requestMessage);
                                this.UpdateAffinityCycle(routingKey, requestMessage, nextAffinityCycle);
                                return this.RequeueMessage(
                                    routingKey,
                                    requestMessage,
                                    responderConfiguration.RequeueDelay);
                            }

                            this.InfoWrite(
                                routingKey,
                                requestMessage.MessageId,
                                string.Format(
                                    "Abandoning request - affinity of consumer does not match ({0})",
                                    messageAffinity));

                            return requestMessage.AbandonAsync();
                        }

                        this.InfoWrite(
                            routingKey,
                            requestMessage.MessageId,
                            string.Format("Processing request with matched affinity {0}", messageAffinity));
                    }

                    TRequest messageBody;
                    try
                    {
                        var content = requestMessage.GetBody<string>();
                        messageBody = this.serializer.StringToMessage<TRequest>(content);
                    }
                    catch (Exception ex)
                    {
                        return this.HandleUnexpectedException(routingKey, requestMessage, ex);
                    }

                    // Renew message lock every 10 seconds
                    var timer = new Timer(requestMessage.KeepLockAlive(this.logger));
                    timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

                    return responder(messageBody).ContinueWith(
                            task =>
                            {
                                timer.Dispose();

                                var responseQueue =
                                    this.advancedBus.QueueDeclare(
                                        requestMessage.Properties[ReplyToKey] as string);

                                if (task.IsFaulted)
                                {
                                    if (task.Exception != null)
                                    {
                                        var body = Activator.CreateInstance<TResponse>();
                                        var dummyContent = this.serializer.MessageToString(body);

                                        var ex = task.Exception.Flatten();
                                        var exception = ex.InnerExceptions.First();
                                        var exceptionMessage = ex.InnerExceptions.First().Message;
                                        var serializedException = JsonConvert.SerializeObject(
                                            ex,
                                            Formatting.None);

                                        this.ErrorWrite(
                                            routingKey,
                                            requestMessage.MessageId,
                                            string.Format(
                                                "Request with correlation id: {0} has faulted - {1}",
                                                requestMessage.CorrelationId,
                                                exceptionMessage));

                                        this.logger.ErrorWrite(exception);

                                        var errorResponseMessage = new BrokeredMessage(dummyContent);
                                        errorResponseMessage.Properties.Add(IsFaultedKey, true);
                                        errorResponseMessage.Properties.Add(ExceptionMessageKey, exceptionMessage);
                                        errorResponseMessage.Properties.Add(SerializedExceptionKey, serializedException);
                                        errorResponseMessage.CorrelationId = requestMessage.CorrelationId;

                                        return responseQueue.SendAsync(errorResponseMessage);
                                    }
                                }

                                var response = this.serializer.MessageToString(task.Result);
                                var responseMessage = new BrokeredMessage(response)
                                                          {
                                                              CorrelationId =
                                                                  requestMessage
                                                                  .CorrelationId
                                                          };

                                this.InfoWrite(
                                    routingKey,
                                    requestMessage.MessageId,
                                    string.Format(
                                        "Request with correlation id {0} completed succesfully",
                                        requestMessage.CorrelationId));

                                return responseQueue.SendAsync(responseMessage);
                            }).ContinueWith(
                                o =>
                                    {
                                        if (o.IsFaulted && o.Exception != null)
                                        {
                                            var ex = o.Exception.Flatten().InnerExceptions.First();
                                            return this.HandleUnexpectedException(routingKey, requestMessage, ex);
                                        }

                                        return requestMessage.CompleteAsync();
                                    });
                    
                },
                onMessageOptions);

            return null;
        }

        private static void ResetMessageSeenCount(
            ConcurrentDictionary<string, int> messageTracker,
            BrokeredMessage message)
        {
            int i;
            messageTracker.TryRemove(message.MessageId, out i);
        }

        private static int TrackMessageSeenCount(
            ConcurrentDictionary<string, int> messageTracker,
            BrokeredMessage message)
        {
            var latest = 1;
            messageTracker.AddOrUpdate(message.MessageId, id => latest, (id, count) => latest = count + 1);
            return latest;
        }

        private static int GetAffinityCycle(BrokeredMessage message)
        {
            var affinityCycle = 0;
            if (message.Properties.ContainsKey("AffinityCycle"))
            {
                int.TryParse(message.Properties["AffinityCycle"].ToString(), out affinityCycle);
            }

            return affinityCycle;
        }

        private Task HandleUnexpectedException(
            string routingKey,
            BrokeredMessage requestMessage,
            Exception ex)
        {
            this.ErrorWrite(
                routingKey,
                requestMessage.MessageId,
                string.Format(
                    "Request with correlation id {0} has faulted unexpectedly: {1}",
                    requestMessage.CorrelationId,
                    ex.Message));

            this.logger.ErrorWrite(ex);

            // deadletter this message if anything unusual happens
            return requestMessage.DeadLetterAsync();
        }

        private string SubscribeToResponse<TRequest, TResponse>()
            where TResponse : class
        {
            var rpcKey = new RpcKey { Request = typeof(TRequest), Response = typeof(TResponse) };

            this.responseQueues.AddOrUpdate(
                rpcKey,
                key =>
                {
                    var name = conventions.RpcReturnQueueNamingConvention();
                    var queue = advancedBus.QueueDeclare(name, autoDelete: true);
                    var onMessageOptions = new OnMessageOptions
                                               {
                                                   AutoComplete = true,
                                                   MaxConcurrentCalls = this.connectionConfiguration.MaxConcurrentCalls
                                               };

                    onMessageOptions.ExceptionReceived += this.exceptionHandler.ExceptionReceived;

                    queue.OnMessageAsync(
                        message => Task.Factory.StartNew(
                            () =>
                            {
                                ResponseAction responseAction;
                                if (responseActions.TryRemove(message.CorrelationId, out responseAction))
                                {
                                    responseAction.OnSuccess(message);
                                }
                            }),
                        onMessageOptions);

                    return name;
                },
                (_, queueName) => queueName);

            return this.responseQueues[rpcKey];
        }

        private void RequestPublish<TRequest>(
            TRequest request,
            string returnQueueName,
            Guid correlationId,
            IRequestConfiguration configuration) where TRequest : class
        {
            var content = this.serializer.MessageToString(request);
            var requestMessage = new BrokeredMessage(content)
                                     {
                                         CorrelationId = correlationId.ToString(),
                                         TimeToLive =
                                             TimeSpan.FromMilliseconds(
                                                 this.connectionConfiguration.Timeout * 1000)
                                     };

            requestMessage.Properties.Add(ReplyToKey, returnQueueName);
            if (configuration.Affinity.HasValue)
            {
                requestMessage.Properties.Add("Affinity", configuration.Affinity.Value);
            }

            var routingKey = this.conventions.RpcRoutingKeyNamingConvention(typeof(TRequest));
            var queue = this.advancedBus.QueueDeclare(routingKey);

            this.InfoWrite(
                routingKey,
                requestMessage.MessageId,
                string.Format("Sending request with correlation id {0}: {1}", correlationId, content));

            queue.SendAsync(requestMessage);
        }

        private void UpdateAffinityCycle(string queueName, BrokeredMessage message, int nextAffinityCycle)
        {
            this.InfoWrite(
               queueName,
               message.MessageId,
               string.Format("Setting affinity cycle {0}", nextAffinityCycle));

            if (!message.Properties.ContainsKey("AffinityCycle"))
            {
                message.Properties.Add("AffinityCycle", nextAffinityCycle);
            }
            else
            {
                message.Properties["AffinityCycle"] = nextAffinityCycle;
            }
        }

        private Task RequeueMessage(string queueName, BrokeredMessage message, int requeueDelay)
        {
            this.InfoWrite(
                queueName,
                message.MessageId,
                string.Format("Re-queueing request in {0} seconds", requeueDelay));

            message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(requeueDelay);

            var queueClient = this.advancedBus.QueueDeclare(queueName);
            return queueClient.SendAsync(message.Clone()).ContinueWith(o => message.Complete());
        }

        private void InfoWrite(string queueName, string messageId, string logMessage)
        {
            this.logger.InfoWrite("{0} - {1}: {2}", queueName, messageId, logMessage);
        }

        private void ErrorWrite(string queueName, string messageId, string logMessage)
        {
            this.logger.ErrorWrite("{0} - {1}: {2}", queueName, messageId, logMessage);
        }
    }
}