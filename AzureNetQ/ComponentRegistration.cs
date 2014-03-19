using System;
using AzureNetQ.Consumer;
using AzureNetQ.Loggers;
using AzureNetQ.Producer;

namespace AzureNetQ
{
    /// <summary>
    /// Registers the default AzureNetQ components in our internal super-simple IoC container.
    /// </summary>
    public class ComponentRegistration
    {
        public static void RegisterServices(IContainer container)
        {
            Preconditions.CheckNotNull(container, "container");

            // Note: IConnectionConfiguration gets registered when RabbitHutch.CreateBus(..) is run.

            // default service registration
            container
                .Register(_ => container)
                .Register<IAzureNetQLogger, ConsoleLogger>()
                .Register<ISerializer, JsonSerializer>()
                .Register<IConventions, Conventions>()
                .Register<IEventBus, EventBus>()
                .Register<ITypeNameSerializer, TypeNameSerializer>()
                .Register<Func<string>>(x => CorrelationIdGenerator.GetCorrelationId)
                .Register<IClusterHostSelectionStrategy<ConnectionFactoryInfo>, DefaultClusterHostSelectionStrategy<ConnectionFactoryInfo>>()
                .Register<IConsumerDispatcherFactory, ConsumerDispatcherFactory>()
                .Register<IPublishExchangeDeclareStrategy, PublishExchangeDeclareStrategy>()
                .Register<IPublisherConfirms, PublisherConfirms>()
                .Register<IConsumerErrorStrategy, DefaultConsumerErrorStrategy>()
                .Register<IHandlerRunner, HandlerRunner>()
                .Register<IInternalConsumerFactory, InternalConsumerFactory>()
                .Register<IConsumerFactory, ConsumerFactory>()
                .Register<IConnectionFactory, ConnectionFactoryWrapper>()
                .Register<IPersistentChannelFactory, PersistentChannelFactory>()
                .Register<IClientCommandDispatcherFactory, ClientCommandDispatcherFactory>()
                .Register<IHandlerCollectionFactory, HandlerCollectionFactory>()
                .Register<IAdvancedBus, RabbitAdvancedBus>()
                .Register<IRpc, Rpc>()
                .Register<ISendReceive, SendReceive>()
                .Register<IBus, RabbitBus>();
        }
         
    }
}