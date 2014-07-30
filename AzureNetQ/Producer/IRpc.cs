namespace AzureNetQ.Producer
{
    using System;
    using System.Threading.Tasks;

    using AzureNetQ.FluentConfiguration;

    /// <summary>
    /// An RPC style request-response pattern
    /// </summary>
    public interface IRpc
    {
        Task<TResponse> Request<TRequest, TResponse>(TRequest request) where TRequest : class
            where TResponse : class;

        Task<TResponse> Request<TRequest, TResponse>(TRequest request, Action<IRequestConfiguration> configure)
            where TRequest : class
            where TResponse : class;

        IDisposable Respond<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : class where TResponse : class;

        IDisposable Respond<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder, Action<IRespondConfiguration> configure)
            where TRequest : class
            where TResponse : class;
    }
}