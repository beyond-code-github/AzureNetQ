namespace AzureNetQ.FluentConfiguration
{
    public interface ISubscriptionConfiguration
    {
        ISubscriptionConfiguration WithTopic(string topic);

        ISubscriptionConfiguration WithSubscription(string subscription);
    }

    public class SubscriptionConfiguration : ISubscriptionConfiguration
    {
        public string Topic { get; private set; }

        public string Subscription { get; private set; }

        public SubscriptionConfiguration()
        {
            this.Subscription = "Default";
        }

        public ISubscriptionConfiguration WithTopic(string topic)
        {
            this.Topic = topic;
            return this;
        }

        public ISubscriptionConfiguration WithSubscription(string subscription)
        {
            this.Subscription = subscription;
            return this;
        }
    }
}