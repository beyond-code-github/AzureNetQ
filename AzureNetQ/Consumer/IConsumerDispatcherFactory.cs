namespace AzureNetQ.Consumer
{
    using System;

    public interface IConsumerDispatcherFactory : IDisposable
    {
        IConsumerDispatcher GetConsumerDispatcher();

        void OnDisconnected();
    }
}