namespace AzureNetQ.AutoSubscribe
{
    using System;

    [Serializable]
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscriptionAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
