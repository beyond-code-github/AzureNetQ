namespace AzureNetQ.AutoSubscribe
{
    using System;

    public interface IAutoSubscriptionConfiguration
    {
        Func<int, bool> AffinityResolver { get; set; }

        IAutoSubscriptionConfiguration WithAffinityResolver(Func<int, bool> affinityResolver);
    }

    public class AutoSubscriptionConfiguration : IAutoSubscriptionConfiguration
    {
        public Func<int, bool> AffinityResolver { get; set; }

        public IAutoSubscriptionConfiguration WithAffinityResolver(Func<int, bool> affinityResolver)
        {
            this.AffinityResolver = affinityResolver;
            return this;
        }
    }
}
