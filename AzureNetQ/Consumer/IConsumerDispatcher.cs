namespace AzureNetQ.Consumer
{
    using System;

    public interface IConsumerDispatcher : IDisposable
    {
        void QueueAction(Action action);

        void OnDisconnected();
    }
}