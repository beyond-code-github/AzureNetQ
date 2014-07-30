namespace AzureNetQ.Consumer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class HandlerCollection : IHandlerCollection
    {
        private readonly IDictionary<Type, object> handlers = new Dictionary<Type, object>();

        private readonly IAzureNetQLogger logger;

        public HandlerCollection(IAzureNetQLogger logger)
        {
            Preconditions.CheckNotNull(logger, "logger");

            this.logger = logger;
            this.ThrowOnNoMatchingHandler = true;
        }

        public bool ThrowOnNoMatchingHandler { get; set; }

        public IHandlerRegistration Add<T>(Func<T, Task> handler) where T : class
        {
            Preconditions.CheckNotNull(handler, "handler");

            if (this.handlers.ContainsKey(typeof(T)))
            {
                throw new AzureNetQException("There is already a handler for message type '{0}'", typeof(T).Name);
            }

            this.handlers.Add(typeof(T), handler);
            return this;
        }

        public IHandlerRegistration Add<T>(Action<T> handler) where T : class
        {
            Preconditions.CheckNotNull(handler, "handler");

            this.Add<T>(message => TaskHelpers.ExecuteSynchronously(() => handler(message)));
            return this;
        }

        // NOTE: refactoring tools might suggest this method is never invoked. Ignore them it
        // _is_ invoked by the GetHandler(Type messsageType) method below by reflection.
        public Func<T, Task> GetHandler<T>() where T : class
        {
            // return (Func<IMessage<T>, MessageReceivedInfo, Task>)GetHandler(typeof(T));
            var messageType = typeof(T);

            if (this.handlers.ContainsKey(messageType))
            {
                return (Func<T, Task>)this.handlers[messageType];
            }

            // no exact handler match found, so let's see if we can find a handler that
            // handles a supertype of the consumed message.
            foreach (var handlerType in this.handlers.Keys.Where(type => type.IsAssignableFrom(messageType)))
            {
                return (Func<T, Task>)this.handlers[handlerType];
            }

            if (this.ThrowOnNoMatchingHandler)
            {
                this.logger.ErrorWrite("No handler found for message type {0}", messageType.Name);
                throw new AzureNetQException("No handler found for message type {0}", messageType.Name);
            }

            return message => Task.Factory.StartNew(() => { });
        }

        public dynamic GetHandler(Type messageType)
        {
            Preconditions.CheckNotNull(messageType, "messageType");

            var getHandlerGenericMethod = GetType().GetMethod("GetHandler", new Type[0]).MakeGenericMethod(messageType);
            return getHandlerGenericMethod.Invoke(this, new object[0]);
        }
    }
}
