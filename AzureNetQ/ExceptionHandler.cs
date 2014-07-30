namespace AzureNetQ
{
    using Microsoft.ServiceBus.Messaging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IExceptionHandler
    {
        void ExceptionReceived(object sender, ExceptionReceivedEventArgs args);
    }

    public class ExceptionHandler : IExceptionHandler
    {
        private IExceptionReporter exceptionReporter;

        public ExceptionHandler(IExceptionReporter exceptionReporter)
        {
            this.exceptionReporter = exceptionReporter;
        }

        public void ExceptionReceived(object sender, ExceptionReceivedEventArgs args)
        {
            this.exceptionReporter.ExceptionReceived(sender, args.Exception);
        }
    }
}