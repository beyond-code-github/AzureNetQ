namespace AzureNetQ
{
    using Microsoft.ServiceBus.Messaging;

    public class NullExceptionReporter : IExceptionReporter
    {
        public void ExceptionReceived(object sender, System.Exception e)
        {
        }
    }
}