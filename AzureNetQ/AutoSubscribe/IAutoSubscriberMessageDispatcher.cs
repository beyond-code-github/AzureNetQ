namespace AzureNetQ.AutoSubscribe
{
    using System.Threading.Tasks;

    public interface IAutoSubscriberMessageDispatcher
    {
        void Dispatch<TMessage, TConsumer>(TMessage message)
            where TMessage : class
            where TConsumer : IConsume<TMessage>;

        Task DispatchAsync<TMessage, TConsumer>(TMessage message)
            where TMessage : class
            where TConsumer : IConsumeAsync<TMessage>;

        TResponse Handle<TMessage, TResponse, TResponder>(TMessage message)
            where TMessage : class
            where TResponder : IRespond<TMessage, TResponse>;

        Task<TResponse> HandleAsync<TMessage, TResponse, TResponder>(TMessage message)
            where TMessage : class
            where TResponder : IRespondAsync<TMessage, TResponse>;
    }
}