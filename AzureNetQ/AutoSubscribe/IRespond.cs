namespace AzureNetQ.AutoSubscribe
{
    public interface IRespond<in T, out TResponse> where T : class
    {
        TResponse Respond(T message);
    }
}