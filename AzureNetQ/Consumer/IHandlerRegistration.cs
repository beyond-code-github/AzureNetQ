namespace AzureNetQ.Consumer
{
    using System;
    using System.Threading.Tasks;

    public interface IHandlerRegistration
    {
        /// <summary>
        /// Set to true if the handler collection should throw an AzureNetQException when no
        /// matching handler is found, or false if it should return a noop handler.
        /// Default is true.
        /// </summary>
        bool ThrowOnNoMatchingHandler { get; set; }

        /// <summary>
        /// Add an asynchronous handler
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="handler">The handler</param>
        /// <returns></returns>
        IHandlerRegistration Add<T>(Func<T, Task> handler)
            where T : class;

        /// <summary>
        /// Add a synchronous handler
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="handler">The handler</param>
        /// <returns></returns>
        IHandlerRegistration Add<T>(Action<T> handler)
            where T : class;
    }
}