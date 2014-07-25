namespace AzureNetQ.AutoSubscribe
{
    using System.Threading.Tasks;

    public interface IConsumeAsync<in T> where T : class 
    {
        Task Consume(T message);
    }
}