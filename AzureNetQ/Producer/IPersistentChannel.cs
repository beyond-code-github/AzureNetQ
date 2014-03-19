using System;
using RabbitMQ.Client;

namespace AzureNetQ.Producer
{
    public interface IPersistentChannel : IDisposable
    {
        void InvokeChannelAction(Action<IModel> channelAction);
    }
}