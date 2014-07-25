namespace AzureNetQ.Consumer
{
    public interface IConsumerConfiguration
    {
        int Priority { get; }

        IConsumerConfiguration WithPriority(int priority);
    }

    public class ConsumerConfiguration : IConsumerConfiguration
    {
        public ConsumerConfiguration()
        {
            this.Priority = 0;
        }

        public int Priority { get; private set; }

        public IConsumerConfiguration WithPriority(int priority)
        {
            this.Priority = priority;
            return this;
        }
    }
}