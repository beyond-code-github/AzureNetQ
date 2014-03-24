namespace AzureNetQ
{
    public interface IMessage<out T>
    {
        MessageProperties Properties { get; }

        T Body { get; }
    }

    public class MessageProperties
    {
        public string CorrelationId { get; set; }
    }
}