using System;
using AzureNetQ.FluentConfiguration;

namespace AzureNetQ.AutoSubscribe
{
    public interface IConsume<in T> where T : class
    {
        void Consume(T message);
    }

    
}