namespace AzureNetQ
{
    public interface IMessage<out T>
    {
        T Body { get; }
    }
}