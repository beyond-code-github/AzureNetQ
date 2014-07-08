namespace AzureNetQ
{
    using System;
    using System.Threading.Tasks;

    using AzureNetQ.Consumer;
    using AzureNetQ.FluentConfiguration;
    using AzureNetQ.Producer;

    using Microsoft.ServiceBus.Messaging;

    public class AzureBus : IBus
    {
        private readonly IAzureNetQLogger logger;
        
        private readonly IConventions conventions;

        private readonly IRpc rpc;

        private readonly ISendReceive sendReceive;

        private readonly IAzureAdvancedBus advancedBus;

        private readonly IConnectionConfiguration connectionConfiguration;

        private readonly ISerializer serializer;

        public AzureBus(
            IAzureNetQLogger logger,
            IConventions conventions,
            IRpc rpc,
            ISendReceive sendReceive,
            IAzureAdvancedBus advancedBus,
            IConnectionConfiguration connectionConfiguration,
            ISerializer serializer)
        {
            Preconditions.CheckNotNull(logger, "logger");
            Preconditions.CheckNotNull(conventions, "conventions");
            Preconditions.CheckNotNull(rpc, "rpc");
            Preconditions.CheckNotNull(sendReceive, "sendReceive");
            Preconditions.CheckNotNull(advancedBus, "advancedBus");
            Preconditions.CheckNotNull(connectionConfiguration, "connectionConfiguration");
            Preconditions.CheckNotNull(serializer, "serializer");

            this.logger = logger;
            this.conventions = conventions;
            this.rpc = rpc;
            this.sendReceive = sendReceive;
            this.advancedBus = advancedBus;
            this.connectionConfiguration = connectionConfiguration;
            this.serializer = serializer;
        }

        public IAzureNetQLogger Logger
        {
            get { return this.logger; }
        }

        public IConventions Conventions
        {
            get { return this.conventions; }
        }

        public void Publish<T>(T message) where T : class
        {
            Preconditions.CheckNotNull(message, "message");

            this.PublishAsync(message).Wait();
        }

        public void Publish<T>(T message, Action<IPublishConfiguration> configure) where T : class
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(configure, "configure");

            this.PublishAsync(message, configure).Wait();
        }

        public void Publish(Type type, object message, Action<IPublishConfiguration> configure)
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(configure, "topic");

            this.PublishAsync(type, message, configure).Wait();
        }

        public Task PublishAsync<T>(T message) where T : class
        {
            Preconditions.CheckNotNull(message, "message");

            return this.PublishAsync(message, x => { });
        }

        public Task PublishAsync<T>(T message, Action<IPublishConfiguration> configure) where T : class
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(configure, "configure");

            return this.PublishAsync(typeof(T), message, configure);
        }

        public Task PublishAsync(Type type, object message)
        {
            Preconditions.CheckNotNull(type, "type");
            Preconditions.CheckNotNull(message, "message");

            return this.PublishAsync(type, message, x => { });
        }

        public Task PublishAsync(Type type, object message, Action<IPublishConfiguration> configure)
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(configure, "configure");
            
            var queueName = this.conventions.TopicNamingConvention(type);
            var queue = this.advancedBus.TopicFind(queueName);

            if (queue != null)
            {
                var configuration = new PublishConfiguration();
                configure(configuration);

                var content = this.serializer.MessageToString(message);
                var azureNetQMessage = new BrokeredMessage(content);

                if (!string.IsNullOrEmpty(configuration.MessageId))
                {
                    azureNetQMessage.MessageId = configuration.MessageId;
                }

                return queue.SendAsync(azureNetQMessage);
            }

            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(false);
            return tcs.Task;
        }

        public virtual void Subscribe<T>(Action<T> onMessage) where T : class
        {
            this.Subscribe(onMessage, x => { });
        }

        public virtual void Subscribe<T>(Action<T> onMessage, Action<ISubscriptionConfiguration> configure) where T : class
        {
            Preconditions.CheckNotNull(onMessage, "onMessage");
            Preconditions.CheckNotNull(configure, "configure");

            this.SubscribeAsync<T>(
                msg =>
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
            this.SubscribeAsync(onMessage, x => { });
        }

        public virtual void SubscribeAsync<T>(Func<T, Task> onMessage, Action<ISubscriptionConfiguration> configure) where T : class
        {
            Preconditions.CheckNotNull(onMessage, "onMessage");
            Preconditions.CheckNotNull(configure, "configure");

            var configuration = new SubscriptionConfiguration();
            configure(configuration);

            var topicName = this.conventions.TopicNamingConvention(typeof(T));
            var subscriptionClient = this.advancedBus.SubscriptionDeclare(
                topicName,
                configuration.Subscription,
                configuration.ReceiveMode,
                configuration.RequiresDuplicateDetection);

            subscriptionClient.OnMessageAsync(
                message =>
                    {
                        var content = message.GetBody<string>();
                        var messageBody = serializer.StringToMessage<T>(content);
                        return onMessage(messageBody);
                    },
                new OnMessageOptions { AutoComplete = true, MaxConcurrentCalls = this.connectionConfiguration.MaxConcurrentCalls });
        }
        
        public TResponse Request<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(request, "request");

            var task = this.RequestAsync<TRequest, TResponse>(request);
            task.Wait();
            return task.Result;
        }

        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class
        {
            Preconditions.CheckNotNull(request, "request");

            return this.rpc.Request<TRequest, TResponse>(request);
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

            this.rpc.Respond(responder);
        }

        public void Send<T>(string queue, T message)
            where T : class
        {
            this.sendReceive.Send(queue, message);
        }

        public IDisposable Receive<T>(string queue, Action<T> onMessage)
            where T : class
        {
            return this.sendReceive.Receive(queue, onMessage);
        }

        public IDisposable Receive<T>(string queue, Func<T, Task> onMessage)
            where T : class
        {
            return this.sendReceive.Receive(queue, onMessage);
        }

        public IDisposable Receive(string queue, Action<IReceiveRegistration> addHandlers)
        {
            return this.sendReceive.Receive(queue, addHandlers);
        }

        public virtual void Dispose()
        {
            // throw new NotImplementedException();
        }
    }
}