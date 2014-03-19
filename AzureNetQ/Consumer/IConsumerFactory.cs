using System;
using System.Threading.Tasks;
using AzureNetQ.Topology;

namespace AzureNetQ.Consumer
{
    public interface IConsumerFactory : IDisposable
    {
        IConsumer CreateConsumer(
            IQueue queue,
            IConsumerConfiguration configuration);
    }
}