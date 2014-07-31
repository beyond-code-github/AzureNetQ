namespace AzureNetQ
{
    using Microsoft.ServiceBus.Messaging;

    public interface IExceptionHandler
    {
        void ExceptionReceived(object sender, ExceptionReceivedEventArgs args);
    }

    public class ExceptionHandler : IExceptionHandler
    {
        private readonly IAzureNetQLogger logger;

        public ExceptionHandler(IAzureNetQLogger logger)
        {
            this.logger = logger;
        }

        public void ExceptionReceived(object sender, ExceptionReceivedEventArgs args)
        {
            this.logger.ErrorWrite(args.Exception);
        }
    }
}