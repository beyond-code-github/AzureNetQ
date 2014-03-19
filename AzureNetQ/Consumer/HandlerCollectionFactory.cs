namespace AzureNetQ.Consumer
{
    public class HandlerCollectionFactory : IHandlerCollectionFactory
    {
        private readonly IAzureNetQLogger logger;

        public HandlerCollectionFactory(IAzureNetQLogger logger)
        {
            this.logger = logger;
        }

        public IHandlerCollection CreateHandlerCollection()
        {
            return new HandlerCollection(logger);
        }
    }
}