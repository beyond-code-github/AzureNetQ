namespace AzureNetQ.FluentConfiguration
{
    using Microsoft.ServiceBus.Messaging;

    public interface ISubscriptionConfiguration
    {
        ISubscriptionConfiguration WithTopic(string topic);

        ISubscriptionConfiguration WithSubscription(string subscription);

        ISubscriptionConfiguration InReadAndDeleteMode();

        ISubscriptionConfiguration WithDuplicateDetection();
    }

    public class SubscriptionConfiguration : ISubscriptionConfiguration
    {
        public string Topic { get; private set; }

        public string Subscription { get; private set; }

        public ReceiveMode ReceiveMode { get; private set; }

        public bool RequiresDuplicateDetection { get; set; }

        public SubscriptionConfiguration()
        {
            this.Subscription = "Default";
            this.ReceiveMode = ReceiveMode.PeekLock;
            this.RequiresDuplicateDetection = false;
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

        public ISubscriptionConfiguration WithDuplicateDetection()
        {
            this.RequiresDuplicateDetection = true;
            return this;
        }
    }
}