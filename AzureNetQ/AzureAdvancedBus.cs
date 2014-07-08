namespace AzureNetQ
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public interface IAzureAdvancedBus
    {
        QueueClient QueueDeclare(string name, bool autoDelete = false);

        void QueueDelete(string name);

        TopicClient TopicFind(string name);

        TopicClient TopicDeclare(string name, bool requiresDuplicateDetection);

        void TopicDelete(string topic);

        SubscriptionClient SubscriptionDeclare(
            string name,
            List<string> topic,
            string subscription,
            ReceiveMode receiveMode,
            bool requiresDuplicateDetection,
            int maxDeliveryCount);
    }

    public class AzureAdvancedBus : IAzureAdvancedBus
    {
        private readonly IAzureNetQLogger logger;

        private readonly IConnectionConfiguration configuration;

        private readonly ConcurrentDictionary<string, QueueClient> queues;

        private readonly ConcurrentDictionary<string, TopicClient> topics;

        private readonly ConcurrentDictionary<string, SubscriptionClient> subscriptions;

        private readonly NamespaceManager namespaceManager;

        private readonly MessagingFactory messagingFactory;

        public AzureAdvancedBus(IAzureNetQLogger logger, IConnectionConfiguration configuration)
        {
            this.namespaceManager = NamespaceManager.CreateFromConnectionString(configuration.ConnectionString);

            var pairs = configuration.ConnectionString.Split(';').Select(o => o.Split('=')).Where(o => o.Length > 1);

            var dictionary = pairs.ToDictionary(key => key[0], value => value[1]);
            var address = this.namespaceManager.Address;

            int port;
            if (dictionary.ContainsKey("Endpoint") && dictionary.ContainsKey("RuntimePort")
                && int.TryParse(dictionary["RuntimePort"], out port))
            {
                var template = new Uri(string.Format("{0}", dictionary["Endpoint"]));
                address = new UriBuilder(template.Scheme, template.Host, port, template.PathAndQuery).Uri;
            }

            var mfs = new MessagingFactorySettings
                          {
                              TokenProvider = this.namespaceManager.Settings.TokenProvider,
                              NetMessagingTransportSettings =
                                  {
                                      BatchFlushInterval =
                                          configuration
                                          .BatchingInterval
                                  }
                          };

            this.messagingFactory = MessagingFactory.Create(address, mfs);

            this.queues = new ConcurrentDictionary<string, QueueClient>();
            this.topics = new ConcurrentDictionary<string, TopicClient>();
            this.subscriptions = new ConcurrentDictionary<string, SubscriptionClient>();

            this.logger = logger;
            this.configuration = configuration;
        }

        public virtual QueueClient QueueDeclare(string name, bool autoDelete = false)
        {
            Preconditions.CheckNotNull(name, "name");

            return this.queues.GetOrAdd(
                name,
                s =>
                {
#if DEBUG
                    SslValidator.OverrideValidation();
#endif
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

        public SubscriptionClient SubscriptionDeclare(
            string name,
            List<string> topicNames,
            string subscription,
            ReceiveMode receiveMode,
            bool requiresDuplicateDetection,
            int maxDeliveryCount)
        {
            var topicClient = this.TopicDeclare(name, requiresDuplicateDetection);

            return this.subscriptions.GetOrAdd(
                BuildSubscriptionKey(name, subscription, topicNames),
                s =>
                    {
#if DEBUG
                    SslValidator.OverrideValidation();
#endif
                        var parts = new List<string> { subscription };
                        parts.AddRange(topicNames);

                        var subscriptionId =
                            string.Join("-", parts.Where(o => !string.IsNullOrEmpty(o)))
                                .Replace("#", "_")
                                .Replace("*", "_");

                        if (!namespaceManager.SubscriptionExists(topicClient.Path, subscriptionId))
                        {
                            var description = new SubscriptionDescription(topicClient.Path, subscriptionId)
                                                  {
                                                      MaxDeliveryCount = maxDeliveryCount,
                                                  };

                            if (topicNames.Any())
                            {
                                var expression = string.Join(
                                    " OR ",
                                    topicNames.Select(
                                        o => string.Format("user.topic LIKE '{0}'", TransformWildcards(o))));

                                var filter = new SqlFilter(expression);
                                namespaceManager.CreateSubscription(description, filter);
                            }
                            else
                            {
                                namespaceManager.CreateSubscription(description);
                            }
                        }

                        return messagingFactory.CreateSubscriptionClient(topicClient.Path, subscriptionId, receiveMode);
                    });
        }

        public TopicClient TopicFind(string name)
        {
            var topicClient = this.topics.GetOrAdd(
                name,
                n =>
                {
#if DEBUG
                    SslValidator.OverrideValidation();
#endif
                    return this.namespaceManager.TopicExists(n) ? this.messagingFactory.CreateTopicClient(n) : null;
                });

            return topicClient;
        }

        public TopicClient TopicDeclare(string name, bool requiresDuplicateDetection)
        {
            var topicClient = this.topics.GetOrAdd(
                name,
                n =>
                {
#if DEBUG
                    SslValidator.OverrideValidation();
#endif
                    if (!this.namespaceManager.TopicExists(n))
                    {
                        var description = new TopicDescription(n) { RequiresDuplicateDetection = requiresDuplicateDetection };

                        this.logger.DebugWrite("Declared Topic: '{0}'", n);
                        this.namespaceManager.CreateTopic(description);
                    }

                    var client = this.messagingFactory.CreateTopicClient(n);
                    return client;
                });
            return topicClient;
        }

        public void QueueDelete(string name)
        {
            QueueClient toRemove;
            if (this.queues.TryRemove(name, out toRemove))
            {
#if DEBUG
                SslValidator.OverrideValidation();
#endif
                if (this.namespaceManager.QueueExists(name))
                {
                    this.namespaceManager.DeleteQueue(name);
                }
            }
        }

        public void TopicDelete(string topic)
        {
            TopicClient toRemove;
            if (this.topics.TryRemove(topic, out toRemove))
            {
#if DEBUG
                SslValidator.OverrideValidation();
#endif
                if (this.namespaceManager.TopicExists(topic))
                {
                    this.namespaceManager.DeleteTopic(topic);
                }
            }
        }

        private static string TransformWildcards(string o)
        {
            return o.Replace('#', '%').Replace('*', '%');
        }

        private static string BuildSubscriptionKey(string name, string subscription, IEnumerable<string> topics)
        {
            return string.Format("{0}{1}{2}", name, subscription, string.Join(string.Empty, topics));
        }

        public static class SslValidator
        {
            public static void OverrideValidation()
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    OnValidateCertificate;
                ServicePointManager.Expect100Continue = true;
            }

            private static bool OnValidateCertificate(
                object sender,
                X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors)
            {
                return true;
            }
        }
    }
}