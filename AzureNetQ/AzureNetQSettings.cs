namespace AzureNetQ
{
    using System;

    using AzureNetQ.Loggers;
    using AzureNetQ.Producer;

    public class AzureNetQSettings
    {
        public AzureNetQSettings()
        {
            this.Logger = () => new NullLogger();
            this.SendAndReceive = () => new SendReceive();
            this.Conventions = () => new Conventions(new TypeNameSerializer());
            this.ConnectionConfiguration = () => new ConnectionConfiguration();

            this.AzureAdvancedBus = new Lazy<IAzureAdvancedBus>(
                () => new AzureAdvancedBus(this.Logger(), this.ConnectionConfiguration()));

            this.Rpc = () => new Rpc(this.Conventions(), this.AzureAdvancedBus.Value, this.ConnectionConfiguration());
        }

        public Lazy<IAzureAdvancedBus> AzureAdvancedBus { get; set; }

        public Func<IConnectionConfiguration> ConnectionConfiguration { get; set; }

        public Func<IAzureNetQLogger> Logger { get; set; }

        public Func<IConventions> Conventions { get; set; }

        public Func<IRpc> Rpc { get; set; }

        public Func<ISendReceive> SendAndReceive { get; set; }
    }
}
