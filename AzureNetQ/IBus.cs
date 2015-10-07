namespace AzureNetQ
{
    using Consumer;
    using FluentConfiguration;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a simple Publish/Subscribe and Request/Response API for a message bus.
    /// </summary>
    public interface IBus : IDisposable
    {
        IAzureNetQLogger Logger { get; }

        IConventions Conventions { get; }

        void Publish<T>(T message) where T : class;

        void Publish<T>(T message, DateTime scheduledEnqueueTime) where T : class;

        void Publish<T>(T message, string topic) where T : class;

        void Publish<T>(T message, string topic, DateTime scheduledEnqueueTime) where T : class;

        void Publish<T>(T message, Action<IPublishConfiguration> configure) where T : class;

        void Publish<T>(T message, Action<IPublishConfiguration> configure, DateTime scheduledEnqueueTime) where T : class;

        void Publish<T>(T message, string topic, Action<IPublishConfiguration> configure) where T : class;

        void Publish<T>(T message, string topic, Action<IPublishConfiguration> configure, DateTime scheduledEnqueueTime) where T : class;

        void Publish(Type type, object message);

        void Publish(Type type, object message, DateTime scheduledEnqueueTime);

        void Publish(Type type, object message, string topic);

        void Publish(Type type, object message, string topic, DateTime scheduledEnqueueTime);

        void Publish(Type type, object message, Action<IPublishConfiguration> configure);

        void Publish(Type type, object message, Action<IPublishConfiguration> configure, DateTime scheduledEnqueueTime);

        void Publish(Type type, object message, string topic, Action<IPublishConfiguration> configure);

        void Publish(Type type, object message, string topic, Action<IPublishConfiguration> configure, DateTime scheduledEnqueueTime);

        Task PublishAsync<T>(T message) where T : class;

        Task PublishAsync<T>(T message, DateTime scheduledEnqueueTime) where T : class;

        Task PublishAsync<T>(T message, string topic) where T : class;

        Task PublishAsync<T>(T message, string topic, DateTime scheduledEnqueueTime) where T : class;

        Task PublishAsync<T>(T message, Action<IPublishConfiguration> configure) where T : class;

        Task PublishAsync<T>(T message, Action<IPublishConfiguration> configure, DateTime scheduledEnqueueTime) where T : class;

        Task PublishAsync<T>(T message, string topic, Action<IPublishConfiguration> configure) where T : class;

        Task PublishAsync<T>(T message, string topic, Action<IPublishConfiguration> configure, DateTime scheduledEnqueueTime) where T : class;

        Task PublishAsync(Type type, object message);

        Task PublishAsync(Type type, object message, DateTime scheduledEnqueueTime);

        Task PublishAsync(Type type, object message, string topic);

        Task PublishAsync(Type type, object message, string topic, DateTime scheduledEnqueueTime);

        Task PublishAsync(Type type, object message, Action<IPublishConfiguration> configure);

        Task PublishAsync(Type type, object message, Action<IPublishConfiguration> configure, DateTime scheduledEnqueueTime);

        Task PublishAsync(Type type, object message, string topicName, Action<IPublishConfiguration> configure);

        Task PublishAsync(Type type, object message, string topicName, Action<IPublishConfiguration> configure, DateTime scheduledEnqueueTime);

        void Subscribe<T>(Action<T> onMessage) where T : class;

        void Subscribe<T>(Action<T> onMessage, Action<ISubscriptionConfiguration> configure) where T : class;

        void SubscribeAsync<T>(Func<T, Task> onMessage) where T : class;

        void SubscribeAsync<T>(Func<T, Task> onMessage, Action<ISubscriptionConfiguration> configure) where T : class;

        TResponse Request<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class;

        TResponse Request<TRequest, TResponse>(TRequest request, Action<IRequestConfiguration> configure)
            where TRequest : class
            where TResponse : class;

        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : class
            where TResponse : class;

        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, Action<IRequestConfiguration> configure)
            where TRequest : class
            where TResponse : class;

        void Respond<TRequest, TResponse>(Func<TRequest, TResponse> responder)
            where TRequest : class
            where TResponse : class;

        void Respond<TRequest, TResponse>(Func<TRequest, TResponse> responder, Action<IRespondConfiguration> configure)
            where TRequest : class
            where TResponse : class;

        void RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : class
            where TResponse : class;

        void RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder, Action<IRespondConfiguration> configure)
            where TRequest : class
            where TResponse : class;

        void Send<T>(string queue, T message)
            where T : class;

        IDisposable Receive<T>(string queue, Action<T> onMessage)
            where T : class;

        IDisposable Receive<T>(string queue, Func<T, Task> onMessage)
            where T : class;

        IDisposable Receive(string queue, Action<IReceiveRegistration> addHandlers);
    }
}