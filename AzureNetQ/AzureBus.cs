using System;
using System.Threading.Tasks;
using AzureNetQ.Consumer;
using AzureNetQ.FluentConfiguration;
using AzureNetQ.Producer;

namespace AzureNetQ
{
    using Microsoft.ServiceBus.Messaging;

    public class AzureBus : IBus
    {
        private readonly IAzureNetQLogger logger;
        private readonly IConventions conventions;
        private readonly IRpc rpc;
        private readonly ISendReceive sendReceive;
        private readonly IAzureAdvancedBus advancedBus;

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
            ISendReceive sendReceive, 
            IAzureAdvancedBus advancedBus)
        {
            Preconditions.CheckNotNull(logger, "logger");
            Preconditions.CheckNotNull(conventions, "conventions");
            Preconditions.CheckNotNull(rpc, "rpc");
            Preconditions.CheckNotNull(sendReceive, "sendReceive");
            Preconditions.CheckNotNull(advancedBus, "advancedBus");

            this.logger = logger;
            this.conventions = conventions;
            this.rpc = rpc;
            this.sendReceive = sendReceive;
            this.advancedBus = advancedBus;
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

        public void Publish(Type type, object message, string topic = "")
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(topic, "topic");

            PublishAsync(type, message, topic).Wait();
        }

        public Task PublishAsync<T>(T message) where T : class
        {
            Preconditions.CheckNotNull(message, "message");

            return PublishAsync(message, string.Empty);
        }

        public Task PublishAsync<T>(T message, string topic) where T : class
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(topic, "topic");

            return this.PublishAsync(typeof(T), message, topic);
        }

        public Task PublishAsync(Type type, object message, string topic = "")
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(topic, "topic");

            var queueName = conventions.TopicNamingConvention(type);
            var queue = advancedBus.TopicDeclare(queueName);

            var azureNetQMessage = new BrokeredMessage(message);
            return queue.SendAsync(azureNetQMessage);
        }

        public virtual void Subscribe<T>(Action<T> onMessage) where T : class
        {
            Subscribe(onMessage, x => { });
        }

        public virtual void Subscribe<T>(Action<T> onMessage, Action<ISubscriptionConfiguration> configure) where T : class
        {
            Preconditions.CheckNotNull(onMessage, "onMessage");
            Preconditions.CheckNotNull(configure, "configure");

            SubscribeAsync<T>(msg =>
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

        public virtual void SubscribeAsync<T>(Func<T, Task> onMessage) where T : class
        {
            SubscribeAsync(onMessage, x => { });
        }

        public virtual void SubscribeAsync<T>(Func<T, Task> onMessage, Action<ISubscriptionConfiguration> configure) where T : class
        {
            Preconditions.CheckNotNull(onMessage, "onMessage");
            Preconditions.CheckNotNull(configure, "configure");

            var configuration = new SubscriptionConfiguration();
            configure(configuration);

            var topicName = conventions.TopicNamingConvention(typeof(T));
            var subscriptionClient = advancedBus.SubscriptionDeclare(topicName, configuration.Subscription);

            subscriptionClient.OnMessageAsync(message => onMessage(message.GetBody<T>()));
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

        public virtual void Respond<TRequest, TResponse>(Func<TRequest, TResponse> responder)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(responder, "responder");

            Func<TRequest, Task<TResponse>> taskResponder =
                request => Task<TResponse>.Factory.StartNew(_ => responder(request), null);

            RespondAsync(taskResponder);
        }

        public virtual void RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(responder, "responder");

            rpc.Respond(responder);
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
            //throw new NotImplementedException();
        }
    }
}