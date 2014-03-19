using System;

namespace AzureNetQ
{
    public class CorrelationIdGenerator
    {
        public static string GetCorrelationId()
        {
            return Guid.NewGuid().ToString();
        } 
    }
}