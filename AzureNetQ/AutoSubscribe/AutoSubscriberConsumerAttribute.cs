using System;

namespace AzureNetQ.AutoSubscribe
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method)]
    public class AutoSubscriberConsumerAttribute : Attribute
    {
        public string SubscriptionId { get; set; }
    }
}