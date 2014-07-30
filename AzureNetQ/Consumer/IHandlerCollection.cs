namespace AzureNetQ.Consumer
{
    using System;
    using System.Threading.Tasks;

    public interface IHandlerCollection : IHandlerRegistration
    {
        /// <summary>
        /// Retrieve a handler from the collection.
        /// If a matching handler cannot be found, the handler collection will either throw
        /// an AzureNetQException, or return null, depending on the value of the 
        /// ThrowOnNoMatchingHandler property.
        /// </summary>
        /// <typeparam name="T">The type of handler to return</typeparam>
        /// <returns>The handler</returns>
        Func<T, Task> GetHandler<T>()
            where T : class;

        /// <summary>
        /// Retrieve a handler from the collection.
        /// If a matching handler cannot be found, the handler collection will either throw
        /// an AzureNetQException, or return null, depending on the value of the 
        /// ThrowOnNoMatchingHandler property.
        /// </summary>
        /// <param name="messageType">The type of handler to return</param>
        /// <returns>The handler</returns>
        dynamic GetHandler(Type messageType);
    }
}