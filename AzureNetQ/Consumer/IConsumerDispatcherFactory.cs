using System;

namespace AzureNetQ.Consumer
{
    public interface IConsumerDispatcherFactory : IDisposable
    {
        IConsumerDispatcher GetConsumerDispatcher();
        void OnDisconnected();
    }
}