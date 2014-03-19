using System.Threading.Tasks;

namespace AzureNetQ.AutoSubscribe
{
    public interface IAutoSubscriberMessageDispatcher
    {
        void Dispatch<TMessage, TConsumer>(TMessage message)
            where TMessage : class
            where TConsumer : IConsume<TMessage>;

        Task DispatchAsync<TMessage, TConsumer>(TMessage message)
            where TMessage : class
            where TConsumer : IConsumeAsync<TMessage>;
    }
}