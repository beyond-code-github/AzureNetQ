namespace AzureNetQ
{
    using System;

    public interface IConnectionConfiguration
    {
        ushort Timeout { get; set; }

        int PrefetchCount { get; set; }

        int MaxConcurrentCalls { get; set; }

        TimeSpan BatchingInterval { get; set; }

        string ConnectionString { get; set; }
    }

    public class ConnectionConfiguration : IConnectionConfiguration
    {
        public ConnectionConfiguration()
        {
            this.Timeout = 30;
            this.PrefetchCount = 0;
            this.MaxConcurrentCalls = 1;
            this.BatchingInterval = TimeSpan.FromMilliseconds(20);
        }

        public TimeSpan BatchingInterval { get; set; }

        public string ConnectionString { get; set; }

        public ushort Timeout { get; set; }

        public int PrefetchCount { get; set; }

        public int MaxConcurrentCalls { get; set; }
    }
}
