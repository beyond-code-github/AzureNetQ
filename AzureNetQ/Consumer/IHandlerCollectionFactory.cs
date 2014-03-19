namespace AzureNetQ.Consumer
{
    public interface IHandlerCollectionFactory
    {
        IHandlerCollection CreateHandlerCollection();
    }
}