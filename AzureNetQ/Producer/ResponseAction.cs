namespace AzureNetQ.Producer
{
    using Microsoft.ServiceBus.Messaging;
    using System;

    internal class ResponseAction
    {
        public Action<BrokeredMessage> OnSuccess { get; set; }
    }
}