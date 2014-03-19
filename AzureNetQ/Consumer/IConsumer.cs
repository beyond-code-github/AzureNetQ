using System;

namespace AzureNetQ.Consumer
{
    public interface IConsumer : IDisposable
    {
        IDisposable StartConsuming();
    }
}