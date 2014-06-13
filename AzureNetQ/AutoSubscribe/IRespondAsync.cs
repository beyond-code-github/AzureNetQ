namespace AzureNetQ.AutoSubscribe
{
    using System.Threading.Tasks;

    public interface IRespondAsync<in T, TResponse> where T : class
    {
        Task<TResponse> Respond(T message);
    }
}