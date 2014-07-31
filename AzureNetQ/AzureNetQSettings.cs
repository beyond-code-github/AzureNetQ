namespace AzureNetQ
{
    using System;

    using AzureNetQ.Consumer;
    using AzureNetQ.Loggers;
    using AzureNetQ.Producer;

    public class AzureNetQSettings
    {
        public AzureNetQSettings()
        {
            this.Logger = () => new ConsoleLogger();
            this.Conventions = () => new Conventions(new TypeNameSerializer());
            this.ConnectionConfiguration = new ConnectionConfiguration();
            this.Serializer = () => new JsonSerializer(new TypeNameSerializer());

            this.AzureAdvancedBus = new Lazy<IAzureAdvancedBus>(
                () => new AzureAdvancedBus(this.Logger(), this.ConnectionConfiguration));

            this.Rpc =
                () =>
                new Rpc(
                    this.Conventions(),
                    this.AzureAdvancedBus.Value,
                    this.ConnectionConfiguration,
                    this.Serializer(),
                    this.Logger());

            this.SendAndReceive = 
                () => 
                    new SendReceive(
                        this.AzureAdvancedBus.Value, 
                        this.ConnectionConfiguration,
                        new HandlerCollectionFactory(this.Logger()),
                        new TypeNameSerializer(),
                        this.Serializer());
        }
        
        public Lazy<IAzureAdvancedBus> AzureAdvancedBus { get; set; }

        public IConnectionConfiguration ConnectionConfiguration { get; set; }

        public Func<IAzureNetQLogger> Logger { get; set; }

        public Func<IConventions> Conventions { get; set; }

        public Func<IRpc> Rpc { get; set; }

        public Func<ISendReceive> SendAndReceive { get; set; }

        public Func<ISerializer> Serializer { get; set; }
    }
}
