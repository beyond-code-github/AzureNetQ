using System;

namespace AzureNetQ.Consumer
{
    public interface IConsumerDispatcher : IDisposable
    {
        void QueueAction(Action action);
        void OnDisconnected();
    }
}