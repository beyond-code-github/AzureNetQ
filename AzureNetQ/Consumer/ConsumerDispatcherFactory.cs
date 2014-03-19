using System;

namespace AzureNetQ.Consumer
{
    /// <summary>
    /// The default ConsumerDispatcherFactory. It creates a single dispatch
    /// queue which all consumers share.
    /// </summary>
    public class ConsumerDispatcherFactory : IConsumerDispatcherFactory
    {
        private readonly Lazy<IConsumerDispatcher> dispatcher;

        public ConsumerDispatcherFactory(IAzureNetQLogger logger)
        {
            Preconditions.CheckNotNull(logger, "logger");
            
            dispatcher = new Lazy<IConsumerDispatcher>(() => new ConsumerDispatcher(logger));
        }

        public IConsumerDispatcher GetConsumerDispatcher()
        {
            return dispatcher.Value;
        }

        public void OnDisconnected()
        {
            if (dispatcher.IsValueCreated)
            {
                dispatcher.Value.OnDisconnected();
            }
        }

        public void Dispose()
        {
            if (dispatcher.IsValueCreated)
            {
                dispatcher.Value.Dispose();
            }
        }
    }
}