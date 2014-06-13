namespace AzureNetQ.FluentConfiguration
{
    public interface IPublishConfiguration
    {
        IPublishConfiguration WithMessageId(string messageId);
    }

    public class PublishConfiguration : IPublishConfiguration
    {
        public string MessageId { get; set; }

        public IPublishConfiguration WithMessageId(string messageId)
        {
            this.MessageId = messageId;
            return this;
        }
    }
}
