using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureNetQ.Topology;

namespace AzureNetQ.Producer
{
    /// <summary>
    /// Default implementation of AzureNetQ's request-response pattern
    /// </summary>
    public class Rpc : IRpc
    {
        private readonly IConventions conventions;

        public Rpc(IConventions conventions)
        {
            Preconditions.CheckNotNull(conventions, "conventions");
            this.conventions = conventions;
        }
        
        public Task<TResponse> Request<TRequest, TResponse>(TRequest request) 
            where TRequest : class 
            where TResponse : class
        {
            Preconditions.CheckNotNull(request, "request");

            throw new NotImplementedException();
        }
        
        private struct RpcKey
        {
            public Type Request;
            public Type Response;
        }

        public IDisposable Respond<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder) 
            where TRequest : class 
            where TResponse : class
        {
            Preconditions.CheckNotNull(responder, "responder");

            var routingKey = conventions.RpcRoutingKeyNamingConvention(typeof(TRequest));

            throw new NotImplementedException();

            //var exchange = advancedBus.ExchangeDeclare(conventions.RpcExchangeNamingConvention(), ExchangeType.Direct);
            //var queue = advancedBus.QueueDeclare(routingKey);
            //advancedBus.Bind(exchange, queue, routingKey);

            //return advancedBus.Consume<TRequest>(queue, (requestMessage, messageRecievedInfo) =>
            //    {
            //        var tcs = new TaskCompletionSource<object>();

            //        responder(requestMessage.Body).ContinueWith(task =>
            //            {
            //                throw new NotImplementedException();
            //                //if (task.IsFaulted)
            //                //{
            //                //    if (task.Exception != null)
            //                //    {
            //                //        var body = Activator.CreateInstance<TResponse>();
            //                //        var responseMessage = new Message<TResponse>(body);
            //                //        responseMessage.Properties.Headers.Add(IsFaultedKey, true);
            //                //        responseMessage.Properties.Headers.Add(ExceptionMessageKey, task.Exception.InnerException.Message);
            //                //        responseMessage.Properties.CorrelationId = requestMessage.Properties.CorrelationId;

            //                //        advancedBus.Publish(Exchange.GetDefault(), requestMessage.Properties.ReplyTo, false, false, responseMessage);
            //                //        tcs.SetException(task.Exception);
            //                //    }
            //                //}
            //                //else
            //                //{
            //                //    var responseMessage = new Message<TResponse>(task.Result);
            //                //    responseMessage.Properties.CorrelationId = requestMessage.Properties.CorrelationId;

            //                //    advancedBus.Publish(Exchange.GetDefault(), requestMessage.Properties.ReplyTo, false, false, responseMessage);
            //                //    tcs.SetResult(null);
            //                //}
            //            });

            //        return tcs.Task;
            //    });
        }
    }
}