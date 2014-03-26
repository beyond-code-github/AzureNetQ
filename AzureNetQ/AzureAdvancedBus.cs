namespace AzureNetQ
{
    using System;
    using System.Collections.Concurrent;
    using System.Configuration;
    using System.Linq;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure;

    public interface IAzureAdvancedBus
    {
        QueueClient QueueDeclare(string name, bool autoDelete = false);

        void QueueDelete(string name);
    }

    public class AzureAdvancedBus : IAzureAdvancedBus
    {
        private readonly IAzureNetQLogger logger;

        private readonly IConnectionConfiguration configuration;

        private readonly ConcurrentDictionary<string, QueueClient> queues;

        private readonly NamespaceManager namespaceManager;

        private readonly MessagingFactory messagingFactory;

        public AzureAdvancedBus(IAzureNetQLogger logger, IConnectionConfiguration configuration)
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            this.namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            var pairs = connectionString.Split(';')
                .Select(o => o.Split('='))
                .Where(o => o.Length > 1);

            var dictionary = pairs.ToDictionary(key => key[0], value => value[1]);
            var address = this.namespaceManager.Address;

            int port;
            if (dictionary.ContainsKey("Endpoint") && dictionary.ContainsKey("RuntimePort") && int.TryParse(dictionary["RuntimePort"], out port))
            {
                var template = new Uri(string.Format("{0}", dictionary["Endpoint"]));
                address = new UriBuilder(template.Scheme, template.Host, port, template.PathAndQuery).Uri;
            }
            
            var mfs = new MessagingFactorySettings
                          {
                              TokenProvider = this.namespaceManager.Settings.TokenProvider,
                              NetMessagingTransportSettings =
                                  {
                                      BatchFlushInterval = configuration.BatchingInterval
                                  }
                          };

            this.messagingFactory = MessagingFactory.Create(address, mfs);
            
            this.queues = new ConcurrentDictionary<string, QueueClient>();

            this.logger = logger;
            this.configuration = configuration;
        }

        public virtual QueueClient QueueDeclare(string name, bool autoDelete = false)
        {
            Preconditions.CheckNotNull(name, "name");

            return this.queues.GetOrAdd(name,
                s =>
                {
                    if (!namespaceManager.QueueExists(s))
                    {
                        var description = new QueueDescription(s);
                        if (autoDelete)
                        {
                            description.AutoDeleteOnIdle = TimeSpan.FromMinutes(5);
                        }
                        
                        logger.DebugWrite("Declared Queue: '{0}'", name);
                        namespaceManager.CreateQueue(description);
                    }

                    var client = messagingFactory.CreateQueueClient(s);
                    client.PrefetchCount = configuration.PrefetchCount;
                    
                    return client;
                });
        }

        public void QueueDelete(string name)
        {
            QueueClient toRemove;
            if (this.queues.TryRemove(name, out toRemove))
            {
                if (namespaceManager.QueueExists(name))
                {
                    namespaceManager.DeleteQueue(name);
                }
            }
        }
    }
}
