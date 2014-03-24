namespace AzureNetQ.Producer
{
    using System;

    internal struct RpcKey
    {
        public Type Request;

        public Type Response;
    }
}