using System;
using AzureNetQ.Topology;

namespace AzureNetQ.Producer
{
    public interface IPublishExchangeDeclareStrategy
    {
        IExchange DeclareExchange(IAdvancedBus advancedBus, string exchangeName, string exchangeType);
        IExchange DeclareExchange(IAdvancedBus advancedBus, Type messageType, string exchangeType);        
    }
}