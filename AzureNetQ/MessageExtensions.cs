namespace AzureNetQ
{
    using System;
    using System.Linq;
    using System.Threading;

    using Microsoft.ServiceBus.Messaging;

    internal static class MessageExtensions
    {
        public static bool ShouldBeFilteredByAffinity(
            this BrokeredMessage message,
            Func<int, bool> affinity,
            out int messageAffinity)
        {
            messageAffinity = 0;
            return affinity != null && message.Properties.ContainsKey("Affinity")
                   && int.TryParse(message.Properties["Affinity"].ToString(), out messageAffinity);
        }

        public static TimerCallback KeepLockAlive(this BrokeredMessage message)
        {
            return state => message.RenewLockAsync().ContinueWith(
                t =>
                {
                    if (t.Exception == null)
                    {
                        return;
                    }

                    var exception = t.Exception.Flatten().InnerExceptions.First();
                    if (exception is MessageLockLostException)
                    {
                        return;
                    }

                    throw t.Exception;
                });
        }
    }
}