namespace AzureNetQ.AutoSubscribe
{
    using System;

    [Serializable]
    [AttributeUsage(AttributeTargets.Method)]
    public class ReceiveAndDeleteAttribute : Attribute
    {
    }
}
