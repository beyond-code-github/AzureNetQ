namespace AzureNetQ
{
    using System;

    public delegate string ExchangeNameConvention(Type messageType);

    public delegate string TopicNameConvention(Type messageType);

    public delegate string QueueNameConvention(Type messageType);

    public delegate string RpcRoutingKeyNamingConvention(Type messageType);

    public delegate string ErrorQueueNameConvention();

    public delegate string ErrorExchangeNameConvention(MessageReceivedInfo info);

    public delegate string RpcExchangeNameConvention();

    public delegate string RpcReturnQueueNamingConvention();

    public delegate string ConsumerTagConvention();

    public interface IConventions
    {
        TopicNameConvention TopicNamingConvention { get; set; }

        QueueNameConvention QueueNamingConvention { get; set; }

        RpcRoutingKeyNamingConvention RpcRoutingKeyNamingConvention { get; set; }

        ErrorQueueNameConvention ErrorQueueNamingConvention { get; set; }

        RpcReturnQueueNamingConvention RpcReturnQueueNamingConvention { get; set; }

        ConsumerTagConvention ConsumerTagConvention { get; set; }
    }

    public class Conventions : IConventions
    {
        public Conventions(ITypeNameSerializer typeNameSerializer)
        {
            Preconditions.CheckNotNull(typeNameSerializer, "typeNameSerializer");

            // Establish default conventions.
            this.TopicNamingConvention = messageType => string.Empty;
            this.RpcRoutingKeyNamingConvention = typeNameSerializer.Serialize;
            this.ErrorQueueNamingConvention = () => "AzureNetQ_Default_Error_Queue";
            this.RpcReturnQueueNamingConvention = () => "azurenetq.response." + Guid.NewGuid();
            this.ConsumerTagConvention = () => Guid.NewGuid().ToString();
            this.QueueNamingConvention = messageType =>
                {
                    var typeName = typeNameSerializer.Serialize(messageType);
                    return string.Format("{0}", typeName);
                };
        }

        public TopicNameConvention TopicNamingConvention { get; set; }

        public QueueNameConvention QueueNamingConvention { get; set; }

        public RpcRoutingKeyNamingConvention RpcRoutingKeyNamingConvention { get; set; }

        public ErrorQueueNameConvention ErrorQueueNamingConvention { get; set; }

        public RpcReturnQueueNamingConvention RpcReturnQueueNamingConvention { get; set; }

        public ConsumerTagConvention ConsumerTagConvention { get; set; }
    }
}