namespace AzureNetQ.FluentConfiguration
{
    public interface ISubscriptionConfiguration
    {
        ISubscriptionConfiguration WithTopic(string topic);

        ISubscriptionConfiguration WithSubscription(string subscription);

        ISubscriptionConfiguration InReadAndDeleteMode();
    }

    public class SubscriptionConfiguration : ISubscriptionConfiguration
    {
        public string Topic { get; private set; }

        public string Subscription { get; private set; }

        public ReceiveMode ReceiveMode { get; private set; }

        public SubscriptionConfiguration()
        {
            this.Subscription = "Default";
            this.ReceiveMode = ReceiveMode.PeekLock;
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

        public ISubscriptionConfiguration InReadAndDeleteMode()
        {
            this.ReceiveMode = ReceiveMode.ReceiveAndDelete;
            return this;
        }
    }
}