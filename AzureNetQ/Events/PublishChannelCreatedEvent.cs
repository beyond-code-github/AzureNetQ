using RabbitMQ.Client;

namespace AzureNetQ.Events
{
    public class PublishChannelCreatedEvent
    {
        public IModel Channel { get; private set; }

        public PublishChannelCreatedEvent(IModel channel)
        {
            Channel = channel;
        }
    }
}   