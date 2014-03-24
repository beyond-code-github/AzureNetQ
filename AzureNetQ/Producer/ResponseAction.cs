namespace AzureNetQ.Producer
{
    using System;

    using Microsoft.ServiceBus.Messaging;

    internal class ResponseAction
    {
        public Action<BrokeredMessage> OnSuccess { get; set; }

        public Action OnFailure { get; set; }
    }
}