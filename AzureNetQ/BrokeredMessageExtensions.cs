using Microsoft.ServiceBus.Messaging;

namespace AzureNetQ
{
    internal static class BrokeredMessageExtensions
    {
        private static string MessageTypePropertyKey = "Type";
        
        public static void SetMessageType(this BrokeredMessage message, string typeName)
        {
            Preconditions.CheckNotNull(message, "message");
            Preconditions.CheckNotNull(typeName, "typeName");

            message.Properties[MessageTypePropertyKey] = typeName;
        }

        public static string GetMessageType(this BrokeredMessage message)
        {
            Preconditions.CheckNotNull(message, "message");

            object prop;
            if (message.Properties.TryGetValue(MessageTypePropertyKey, out prop))
            {
                return prop.ToString();
            }

            return null;
        }
    }
}
