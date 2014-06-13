using System;
using System.Threading.Tasks;

namespace AzureNetQ.AutoSubscribe
{
    public class DefaultAutoSubscriberMessageDispatcher : IAutoSubscriberMessageDispatcher
    {
        public void Dispatch<TMessage, TConsumer>(TMessage message) 
            where TMessage : class
            where TConsumer : IConsume<TMessage>
        {
            var consumer = (IConsume<TMessage>)Activator.CreateInstance(typeof(TConsumer));

            consumer.Consume(message);
        }

        public Task DispatchAsync<TMessage, TConsumer>(TMessage message)
            where TMessage : class
            where TConsumer : IConsumeAsync<TMessage>
        {
            var consumer = (IConsumeAsync<TMessage>)Activator.CreateInstance(typeof(TConsumer));

            return consumer.Consume(message);
        }

        public TResponse Handle<TMessage, TResponse, TResponder>(TMessage message)
            where TMessage : class
            where TResponder : IRespond<TMessage, TResponse>
        {
            var responder = (IRespond<TMessage, TResponse>)Activator.CreateInstance(typeof(TResponder));
            return responder.Respond(message);
        }

        public Task<TResponse> HandleAsync<TMessage, TResponse, TResponder>(TMessage message)
            where TMessage : class
            where TResponder : IRespondAsync<TMessage, TResponse>
        {
            var responder = (IRespondAsync<TMessage, TResponse>)Activator.CreateInstance(typeof(TResponder));
            return responder.Respond(message);
        }
    }
}