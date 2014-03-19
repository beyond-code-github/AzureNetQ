using System;
using System.Threading.Tasks;
using AzureNetQ.Topology;

namespace AzureNetQ.Consumer
{
    public interface IConsumerFactory : IDisposable
    {
        IConsumer CreateConsumer(
            IQueue queue, 
            Func<Byte[], MessageProperties, MessageReceivedInfo, Task> onMessage, 
            IPersistentConnection connection,
            IConsumerConfiguration configuration
            );
    }
}