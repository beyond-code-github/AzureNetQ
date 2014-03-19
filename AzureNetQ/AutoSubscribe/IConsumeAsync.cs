using System.Threading.Tasks;

namespace AzureNetQ.AutoSubscribe
{
    public interface IConsumeAsync<in T> where T : class 
    {
        Task Consume(T message);
    }
}