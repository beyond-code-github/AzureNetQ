using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AzureNetQ.Topology;

namespace AzureNetQ.Consumer
{
    public class ConsumerFactory : IConsumerFactory
    {
        private readonly ConcurrentDictionary<IConsumer, object> consumers = new ConcurrentDictionary<IConsumer, object>();

        public ConsumerFactory()
        {
        }

        public IConsumer CreateConsumer(
            IQueue queue, 
            IConsumerConfiguration configuration
            )
        {
            Preconditions.CheckNotNull(queue, "queue");

            var consumer = CreateConsumerInstance(queue, configuration);
            consumers.TryAdd(consumer, null);
            return consumer;
        }

        /// <summary>
        /// Create the correct implementation of IConsumer based on queue properties
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="onMessage"></param>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private IConsumer CreateConsumerInstance(
            IQueue queue, 
            IConsumerConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var consumer in consumers.Keys)
            {
                consumer.Dispose();
            }
        }
    }
}