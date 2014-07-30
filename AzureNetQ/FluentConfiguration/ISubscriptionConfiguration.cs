namespace AzureNetQ.FluentConfiguration
{
    using System.Collections.Generic;

    using Microsoft.ServiceBus.Messaging;

    public interface ISubscriptionConfiguration
    {
        ISubscriptionConfiguration WithTopic(string topic);

        ISubscriptionConfiguration WithSubscription(string subscription);

        ISubscriptionConfiguration InReadAndDeleteMode();

        ISubscriptionConfiguration WithDuplicateDetection();
        
        ISubscriptionConfiguration WithMaxDeliveryCount(int maxDeliveryCount);
    }

    public class SubscriptionConfiguration : ISubscriptionConfiguration
    {
        public SubscriptionConfiguration()
        {
            this.Subscription = "Default";
            this.ReceiveMode = ReceiveMode.PeekLock;
            this.RequiresDuplicateDetection = false;
            this.Topics = new List<string>();
            this.MaxDeliveryCount = 10;
        }

        public List<string> Topics { get; private set; }
        
        public string Subscription { get; private set; }

        public ReceiveMode ReceiveMode { get; private set; }

        public bool RequiresDuplicateDetection { get; set; }

        public int MaxDeliveryCount { get; set; }
        
        public ISubscriptionConfiguration WithTopic(string topic)
        {
            this.Topics.Add(topic);
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

        public ISubscriptionConfiguration WithMaxDeliveryCount(int maxDeliveryCount)
        {
            this.MaxDeliveryCount = maxDeliveryCount;
            return this;
        }
    }
}
