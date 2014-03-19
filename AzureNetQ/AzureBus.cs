using System;
using System.Threading.Tasks;
using AzureNetQ.Consumer;
using AzureNetQ.FluentConfiguration;
using AzureNetQ.Producer;

namespace AzureNetQ
{
    public class AzureBus : IBus
    {
        private readonly IAzureNetQLogger logger;
        private readonly IConventions conventions;
        private readonly IRpc rpc;
        private readonly ISendReceive sendReceive;

        public IAzureNetQLogger Logger
        {
            get { return logger; }
        }

        public IConventions Conventions
        {
            get { return conventions; }
        }

        public AzureBus(
            IAzureNetQLogger logger,
            IConventions conventions,
            IRpc rpc,
            ISendReceive sendReceive)
        {
            Preconditions.CheckNotNull(logger, "logger");
            Preconditions.CheckNotNull(conventions, "conventions");
            Preconditions.CheckNotNull(rpc, "rpc");
            Preconditions.CheckNotNull(sendReceive, "sendReceive");

            this.logger = logger;
            this.conventions = conventions;
            this.rpc = rpc;
            this.sendReceive = sendReceive;
        }

        public void Publish<T>(T message) where T : class
        {
            Preconditions.CheckNotNull(message, "message");

            PublishAsync(message).Wait();
        }

        public void Publish<T>(T message, string topic) where T : class
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(topic, "topic");

            PublishAsync(message, topic).Wait();
        }

        public Task PublishAsync<T>(T message) where T : class
        {
            Preconditions.CheckNotNull(message, "message");

            return PublishAsync(message, conventions.TopicNamingConvention(typeof(T)));
        }

        public Task PublishAsync<T>(T message, string topic) where T : class
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(topic, "topic");
            
            throw new NotImplementedException();
        }

        public virtual IDisposable Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class
        {
            return Subscribe(subscriptionId, onMessage, x => { });
        }

        public virtual IDisposable Subscribe<T>(string subscriptionId, Action<T> onMessage, Action<ISubscriptionConfiguration> configure) where T : class
        {
            Preconditions.CheckNotNull(subscriptionId, "subscriptionId");
            Preconditions.CheckNotNull(onMessage, "onMessage");
            Preconditions.CheckNotNull(configure, "configure");

            return SubscribeAsync<T>(subscriptionId, msg =>
            {
                var tcs = new TaskCompletionSource<object>();
                try
                {
                    onMessage(msg);
                    tcs.SetResult(null);
                }
                catch (Exception exception)
                {
                    tcs.SetException(exception);
                }
                return tcs.Task;
            },
            configure);
        }

        public virtual IDisposable SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage) where T : class
        {
            return SubscribeAsync(subscriptionId, onMessage, x => { });
        }

        public virtual IDisposable SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage, Action<ISubscriptionConfiguration> configure) where T : class
        {
            Preconditions.CheckNotNull(subscriptionId, "subscriptionId");
            Preconditions.CheckNotNull(onMessage, "onMessage");
            Preconditions.CheckNotNull(configure, "configure");

            var configuration = new SubscriptionConfiguration();
            configure(configuration);

            var queueName = conventions.QueueNamingConvention(typeof(T), subscriptionId);
            var exchangeName = conventions.ExchangeNamingConvention(typeof(T));

            throw new NotImplementedException();
        }

        public TResponse Request<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(request, "request");

            var task = RequestAsync<TRequest, TResponse>(request);
            task.Wait();
            return task.Result;
        }

        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(request, "request");

            return rpc.Request<TRequest, TResponse>(request);
        }

        public virtual IDisposable Respond<TRequest, TResponse>(Func<TRequest, TResponse> responder)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(responder, "responder");

            Func<TRequest, Task<TResponse>> taskResponder =
                request => Task<TResponse>.Factory.StartNew(_ => responder(request), null);

            return RespondAsync(taskResponder);
        }

        public virtual IDisposable RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(responder, "responder");

            return rpc.Respond(responder);
        }

        public void Send<T>(string queue, T message)
            where T : class
        {
            sendReceive.Send(queue, message);
        }

        public IDisposable Receive<T>(string queue, Action<T> onMessage)
            where T : class
        {
            return sendReceive.Receive(queue, onMessage);
        }

        public IDisposable Receive<T>(string queue, Func<T, Task> onMessage)
            where T : class
        {
            return sendReceive.Receive(queue, onMessage);
        }

        public IDisposable Receive(string queue, Action<IReceiveRegistration> addHandlers)
        {
            return sendReceive.Receive(queue, addHandlers);
        }
        
        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}