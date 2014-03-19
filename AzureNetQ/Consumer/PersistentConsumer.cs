using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureNetQ.Topology;

namespace AzureNetQ.Consumer
{
    public class PersistentConsumer : IConsumer
    {
        private readonly IQueue queue;
        private readonly IConsumerConfiguration configuration;
        
        public PersistentConsumer(
            IQueue queue, 
            IConsumerConfiguration configuration)
        {
            Preconditions.CheckNotNull(queue, "queue");
            Preconditions.CheckNotNull(configuration, "configuration");

            this.queue = queue;
            this.configuration = configuration;
        }

        public IDisposable StartConsuming()
        {
            throw new NotImplementedException();

            return new ConsumerCancellation(Dispose);
        }
        
        private bool disposed = false;

        public void Dispose()
        {
            if (disposed) return;

            disposed = true;

            throw new NotImplementedException();

            //foreach (var cancelSubscription in eventCancellations)
            //{
            //    cancelSubscription();
            //}

            //foreach (var internalConsumer in internalConsumers.Keys)
            //{
            //    internalConsumer.Dispose();
            //}
        }
    }
}