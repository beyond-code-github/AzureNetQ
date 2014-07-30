namespace AzureNetQ
{
    using Microsoft.ServiceBus.Messaging;
    using System;

    public interface IExceptionReporter
    {
        void ExceptionReceived(object sender, Exception e);
    }
}