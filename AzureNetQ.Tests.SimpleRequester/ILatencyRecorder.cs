using System;

namespace AzureNetQ.Tests.SimpleRequester
{
    public interface ILatencyRecorder : IDisposable
    {
        void RegisterRequest(long requestId);
        void RegisterResponse(long responseId);
    }
}