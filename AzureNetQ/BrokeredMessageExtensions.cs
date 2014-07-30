namespace AzureNetQ
{
    using Microsoft.ServiceBus.Messaging;

    internal static class BrokeredMessageExtensions
    {
        private const string MessageTypePropertyKey = "Type";

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
            return message.Properties.TryGetValue(MessageTypePropertyKey, out prop) ? prop.ToString() : null;
        }
    }
}
