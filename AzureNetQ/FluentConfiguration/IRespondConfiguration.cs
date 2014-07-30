namespace AzureNetQ.FluentConfiguration
{
    using System;

    public interface IRespondConfiguration
    {
        Func<int, bool> AffinityResolver { get; }

        int RequeueDelay { get; set; }

        IRespondConfiguration WithAffinityResolver(Func<int, bool> affinity);

        IRespondConfiguration WithRequeueDelay(int requeueDelay);
    }

    public class RespondConfiguration : IRespondConfiguration
    {
        public RespondConfiguration()
        {
            this.RequeueDelay = 5;
        }

        public Func<int, bool> AffinityResolver { get; private set; }

        public int RequeueDelay { get; set; }

        public IRespondConfiguration WithAffinityResolver(Func<int, bool> affinity)
        {
            this.AffinityResolver = affinity;
            return this;
        }

        public IRespondConfiguration WithRequeueDelay(int requeueDelay)
        {
            this.RequeueDelay = requeueDelay;
            return this;
        }
    }
}