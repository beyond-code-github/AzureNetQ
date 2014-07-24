using AzureNetQ.Consumer;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AzureNetQ.Producer
{
    using Microsoft.ServiceBus.Messaging;

    public class SendReceive : ISendReceive
    {
        private readonly IAzureAdvancedBus advancedBus;
        private readonly IConnectionConfiguration connectionConfiguration;
        private readonly IHandlerCollectionFactory handlerCollectionFactory;
        private readonly ITypeNameSerializer typeNameSerializer;
        private readonly ISerializer serializer;
        
        private readonly ConcurrentDictionary<string, QueueClient> declaredQueues = new ConcurrentDictionary<string, QueueClient>(); 

        public SendReceive(
            IAzureAdvancedBus advancedBus, 
            IConnectionConfiguration connectionConfiguration, 
            IHandlerCollectionFactory handlerCollectionFactory,
            ITypeNameSerializer typeNameSerializer,
            ISerializer serializer)
        {
            Preconditions.CheckNotNull(advancedBus, "advancedBus");
            Preconditions.CheckNotNull(connectionConfiguration, "connectionConfiguration");
            Preconditions.CheckNotNull(handlerCollectionFactory, "handlerCollectionFactory");
            Preconditions.CheckNotNull(typeNameSerializer, "typeNameSerializer");
            Preconditions.CheckNotNull(serializer, "serializer");

            this.advancedBus = advancedBus;
            this.connectionConfiguration = connectionConfiguration;
            this.handlerCollectionFactory = handlerCollectionFactory;
            this.typeNameSerializer = typeNameSerializer;
            this.serializer = serializer;
        }

        public void Send<T>(string queue, T message)
            where T : class
        {
            Preconditions.CheckNotNull(queue, "queue");
            Preconditions.CheckNotNull(message, "message");

            var declaredQueue = DeclareQueue(queue);

            var content = serializer.MessageToString(message);
            var queueMessage = new BrokeredMessage(content);
            queueMessage.SetMessageType(typeNameSerializer.Serialize(typeof(T)));

            declaredQueue.Send(queueMessage);
        }

        public IDisposable Receive<T>(string queue, Action<T> onMessage)
            where T : class
        {
            Preconditions.CheckNotNull(queue, "queue");
            Preconditions.CheckNotNull(onMessage, "onMessage");

            return Receive<T>(queue, message => TaskHelpers.ExecuteSynchronously(() => onMessage(message)));
        }

        public IDisposable Receive<T>(string queue, Func<T, Task> onMessage)
            where T : class
        {
            Preconditions.CheckNotNull(queue, "queue");
            Preconditions.CheckNotNull(onMessage, "onMessage");

            var declaredQueue = DeclareQueue(queue);
            return Consume(declaredQueue, handlers => handlers.Add<T>((wrapped, info) => onMessage(wrapped.Body)));
        }

        public IDisposable Receive(string queue, Action<IReceiveRegistration> addHandlers)
        {
            Preconditions.CheckNotNull(queue, "queue");
            Preconditions.CheckNotNull(addHandlers, "addHandlers");

            var declaredQueue = DeclareQueue(queue);
            return Consume(declaredQueue, x => addHandlers(new HandlerAdder(x)));
        }

        private IDisposable Consume(QueueClient declaredQueue, Action<IHandlerRegistration> addHandlers)
        {
            var handlerCollection = handlerCollectionFactory.CreateHandlerCollection();
            addHandlers(handlerCollection);

            declaredQueue.OnMessageAsync(queueMessage =>
            {
                var messageBody = queueMessage.GetBody<string>();
                var messageType = queueMessage.GetMessageType();

                var message = this.serializer.StringToMessage(messageType, messageBody);
                var handler = handlerCollection.GetHandler(message.GetType());
                var wrappedMessageType = typeof(WrappedMessage<>).MakeGenericType(message.GetType());
                dynamic wrappedMessage = Activator.CreateInstance(wrappedMessageType, message, new MessageProperties());

                return handler(wrappedMessage, new MessageReceivedInfo()); // HACK - not using MessageReceievedInfo
            });

            return null;
        }

        private QueueClient DeclareQueue(string queueName)
        {
            QueueClient queue = null;
            declaredQueues.AddOrUpdate(
                queueName,
                key => queue = advancedBus.QueueDeclare(queueName),
                (key, value) => queue = value
            );

            return queue;
        }

        private class HandlerAdder : IReceiveRegistration
        {
            private readonly IHandlerRegistration handlerRegistration;

            public HandlerAdder(IHandlerRegistration handlerRegistration)
            {
                this.handlerRegistration = handlerRegistration;
            }

            public IReceiveRegistration Add<T>(Func<T, Task> onMessage) where T : class
            {
                handlerRegistration.Add<T>((message, info) => onMessage(message.Body));
                return this;
            }

            public IReceiveRegistration Add<T>(Action<T> onMessage) where T : class
            {
                handlerRegistration.Add<T>((message, info) => onMessage(message.Body));
                return this;
            }
        }

        private class WrappedMessage<T> : IMessage<T>
        {
            public T Body { get; private set; }
            public MessageProperties Properties { get; private set; }
            
            public WrappedMessage(T body, MessageProperties properties)
            {
                Body = body;
                Properties = properties;
            }
        }
    }
}