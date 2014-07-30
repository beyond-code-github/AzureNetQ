namespace AzureNetQ.AutoSubscribe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using AzureNetQ.FluentConfiguration;

    /// <summary>
    /// Lets you scan assemblies for implementations of <see cref="IConsume{T}"/> so that
    /// these will get registrered as subscribers in the bus.
    /// </summary>
    public class AutoSubscriber
    {
        protected const string ConsumeMethodName = "Consume";
        protected const string DispatchMethodName = "Dispatch";
        protected const string DispatchAsyncMethodName = "DispatchAsync";
        protected const string HandleMethodName = "Handle";
        protected const string HandleAsyncMethodName = "HandleAsync";

        protected readonly IBus Bus;

        public AutoSubscriber(IBus bus)
        {
            Preconditions.CheckNotNull(bus, "bus");

            this.Bus = bus;
            this.AutoSubscriberMessageDispatcher = new DefaultAutoSubscriberMessageDispatcher();
        }

        /// <summary>
        /// Responsible for consuming a message with the relevant message consumer.
        /// </summary>
        public IAutoSubscriberMessageDispatcher AutoSubscriberMessageDispatcher { get; set; }

        public virtual void Subscribe(
            Assembly assembly)
        {
            this.Subscribe(assembly, x => { });
        }

        public virtual void Subscribe(
            Assembly assembly, Action<IAutoSubscriptionConfiguration> configuration)
        {
            this.Subscribe(new List<Assembly> { assembly }, configuration);
        }

        public virtual void Subscribe(
            List<Assembly> assemblies)
        {
            this.Subscribe(assemblies, x => { });
        }

        /// <summary>
        /// Registers all consumers in passed assembly. The actual Subscriber instances is
        /// created using <seealso cref="AutoSubscriberMessageDispatcher"/>.
        /// </summary>
        /// <param name="assemblies">The assembleis to scan for consumers.</param>
        /// <param name="configuration"></param>
        public virtual void Subscribe(List<Assembly> assemblies, Action<IAutoSubscriptionConfiguration> configuration)
        {
            Preconditions.CheckAny(assemblies, "assemblies", "No assemblies specified.");

            var autoSubscriptionConfiguration = new AutoSubscriptionConfiguration();
            configuration(autoSubscriptionConfiguration);

            var genericBusSubscribeMethod = this.GetSubscribeMethodOfBus("Subscribe", typeof(Action<>));
            var subscriptionInfos = this.GetConsumerInfos(assemblies.SelectMany(a => a.GetTypes()), typeof(IConsume<>));

            this.InvokeMethods(
                autoSubscriptionConfiguration,
                subscriptionInfos,
                DispatchMethodName,
                genericBusSubscribeMethod,
                messageType => typeof(Action<>).MakeGenericType(messageType));

            var genericBusRespondMethod = this.GetRespondMethodOfBus("Respond", typeof(Func<,>));
            var responderInfos = this.GetResponderInfos(assemblies.SelectMany(a => a.GetTypes()), typeof(IRespond<,>));

            this.InvokeMethods(
                autoSubscriptionConfiguration,
                responderInfos,
                HandleMethodName,
                genericBusRespondMethod,
                (messageType, responseType) => typeof(Func<,>).MakeGenericType(messageType, responseType));
        }

        public virtual void SubscribeAsync(
            Assembly assembly)
        {
            this.SubscribeAsync(assembly, x => { });
        }

        public virtual void SubscribeAsync(
            Assembly assembly, Action<IAutoSubscriptionConfiguration> configuration)
        {
            this.SubscribeAsync(new List<Assembly> { assembly }, configuration);
        }

        public virtual void SubscribeAsync(
            List<Assembly> assemblies)
        {
            this.SubscribeAsync(assemblies, x => { });
        }

        /// <summary>
        /// Registers all async consumers in passed assembly. The actual Subscriber instances is
        /// created using <seealso cref="AutoSubscriberMessageDispatcher"/>.
        /// </summary>
        /// <param name="assemblies">The assembleis to scan for consumers.</param>
        /// <param name="configuration"></param>
        public virtual void SubscribeAsync(List<Assembly> assemblies, Action<IAutoSubscriptionConfiguration> configuration)
        {
            Preconditions.CheckAny(assemblies, "assemblies", "No assemblies specified.");

            var autoSubscriptionConfiguration = new AutoSubscriptionConfiguration();
            configuration(autoSubscriptionConfiguration);

            var genericBusSubscribeMethod = this.GetSubscribeMethodOfBus("SubscribeAsync", typeof(Func<,>));
            var consumerInfos = this.GetConsumerInfos(assemblies.SelectMany(a => a.GetTypes()), typeof(IConsumeAsync<>));
            Func<Type, Type> subscriberTypeFromMessageTypeDelegate =
                messageType => typeof(Func<,>).MakeGenericType(messageType, typeof(Task));

            this.InvokeMethods(
                autoSubscriptionConfiguration,
                consumerInfos,
                DispatchAsyncMethodName,
                genericBusSubscribeMethod,
                subscriberTypeFromMessageTypeDelegate);

            var genericBusRespondMethod = this.GetRespondMethodOfBus("RespondAsync", typeof(Func<,>));
            var responderInfos = this.GetResponderInfos(assemblies.SelectMany(a => a.GetTypes()), typeof(IRespondAsync<,>));
            Func<Type, Type, Type> subscriberTypeFromMessageAndResponseTypeDelegate =
                (messageType, responseType) => typeof(Func<,>).MakeGenericType(messageType, typeof(Task<>).MakeGenericType(responseType));

            this.InvokeMethods(
                autoSubscriptionConfiguration,
               responderInfos,
               HandleAsyncMethodName,
               genericBusRespondMethod,
               subscriberTypeFromMessageAndResponseTypeDelegate);
        }

        protected void InvokeMethods(
            AutoSubscriptionConfiguration autoSubscriptionConfiguration,
            IEnumerable<KeyValuePair<Type, AutoSubscriberConsumerInfo[]>> subscriptionInfos,
            string dispatchName,
            MethodInfo genericBusSubscribeMethod,
            Func<Type, Type> subscriberTypeFromMessageTypeDelegate)
        {
            foreach (var kv in subscriptionInfos)
            {
                foreach (var subscriptionInfo in kv.Value)
                {
                    var dispatchMethod =
                            this.AutoSubscriberMessageDispatcher.GetType()
                                                           .GetMethod(dispatchName, BindingFlags.Instance | BindingFlags.Public)
                                                           .MakeGenericMethod(subscriptionInfo.MessageType, subscriptionInfo.ConcreteType);

                    var configurationActions = new List<Action<ISubscriptionConfiguration>>();
                    var subscriptionAttribute = this.GetSubscriptionAttribute(subscriptionInfo);
                    if (subscriptionAttribute != null)
                    {
                        configurationActions.Add(c => c.WithSubscription(subscriptionAttribute.Name));
                    }

                    var readAndDeleteAttribute = this.GetReadAndDeleteAttribute(subscriptionInfo);
                    if (readAndDeleteAttribute != null)
                    {
                        configurationActions.Add(c => c.InReadAndDeleteMode());
                    }

                    var configuration =
                        new Action<ISubscriptionConfiguration>(c => configurationActions.ForEach(o => o(c)));
                    
                    var dispatchDelegate =
                        Delegate.CreateDelegate(
                            subscriberTypeFromMessageTypeDelegate(subscriptionInfo.MessageType),
                            this.AutoSubscriberMessageDispatcher,
                            dispatchMethod);
                            
                    var busSubscribeMethod = genericBusSubscribeMethod.MakeGenericMethod(subscriptionInfo.MessageType);

                    busSubscribeMethod.Invoke(this.Bus, new object[] { dispatchDelegate, configuration });
                }
            }
        }

        protected virtual SubscriptionAttribute GetSubscriptionAttribute(AutoSubscriberConsumerInfo subscriptionInfo)
        {
            var consumeMethod = this.ConsumeMethod(subscriptionInfo);
            return consumeMethod.GetCustomAttributes(typeof(SubscriptionAttribute), true).SingleOrDefault() as SubscriptionAttribute;
        }

        protected virtual ReceiveAndDeleteAttribute GetReadAndDeleteAttribute(AutoSubscriberConsumerInfo subscriptionInfo)
        {
            var consumeMethod = this.ConsumeMethod(subscriptionInfo);
            return consumeMethod.GetCustomAttributes(typeof(ReceiveAndDeleteAttribute), true).SingleOrDefault() as ReceiveAndDeleteAttribute;
        }

        protected void InvokeMethods(
            AutoSubscriptionConfiguration autoSubscriptionConfiguration,
            IEnumerable<KeyValuePair<Type, AutoSubscriberResponderInfo[]>> subscriptionInfos,
            string handlerName,
            MethodInfo genericBusRespondMethod,
            Func<Type, Type, Type> subscriberTypeFromMessageTypeDelegate)
        {
            foreach (var kv in subscriptionInfos)
            {
                foreach (var subscriptionInfo in kv.Value)
                {
                    var configuration = new Action<IRespondConfiguration>(c => { });
                    if (autoSubscriptionConfiguration.AffinityResolver != null)
                    {
                        configuration = c => c.WithAffinityResolver(autoSubscriptionConfiguration.AffinityResolver);
                    }

                    var handleMethod =
                        this.AutoSubscriberMessageDispatcher.GetType()
                            .GetMethod(handlerName, BindingFlags.Instance | BindingFlags.Public)
                            .MakeGenericMethod(
                                subscriptionInfo.MessageType,
                                subscriptionInfo.ResponseType,
                                subscriptionInfo.ConcreteType);

                    var handleDelegate =
                        Delegate.CreateDelegate(
                            subscriberTypeFromMessageTypeDelegate(
                                subscriptionInfo.MessageType,
                                subscriptionInfo.ResponseType),
                            this.AutoSubscriberMessageDispatcher,
                            handleMethod);

                    var busRespondMethod = genericBusRespondMethod.MakeGenericMethod(
                        subscriptionInfo.MessageType,
                        subscriptionInfo.ResponseType);

                    busRespondMethod.Invoke(this.Bus, new object[] { handleDelegate, configuration });
                }
            }
        }

        protected virtual bool IsValidMarkerType(Type markerType)
        {
            return markerType.IsInterface && markerType.GetMethods().Any(m => m.Name == ConsumeMethodName);
        }

        protected virtual MethodInfo GetSubscribeMethodOfBus(string method, Type paramType)
        {
            return
                this.Bus.GetType()
                    .GetMethods()
                .Where(m => m.Name == method)
                .Select(m => new { Method = m, Params = m.GetParameters() })
                    .Single(
                        m =>
                        m.Params.Length == 2 && m.Params[0].ParameterType.GetGenericTypeDefinition() == paramType
                        && m.Params[1].ParameterType == typeof(Action<ISubscriptionConfiguration>))
                    .Method;
        }

        protected virtual MethodInfo GetRespondMethodOfBus(string method, Type paramType)
        {
            return
                this.Bus.GetType()
                    .GetMethods()
                .Where(m => m.Name == method)
                .Select(m => new { Method = m, Params = m.GetParameters() })
                    .Single(
                        m =>
                        m.Params.Length == 2 && m.Params[0].ParameterType.GetGenericTypeDefinition() == paramType
                        && m.Params[1].ParameterType == typeof(Action<IRespondConfiguration>))
                .Method;
        }

        protected virtual IEnumerable<KeyValuePair<Type, AutoSubscriberConsumerInfo[]>> GetConsumerInfos(IEnumerable<Type> types, Type interfaceType)
        {
            foreach (var concreteType in types.Where(t => t.IsClass))
            {
                var subscriptionInfos = concreteType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                    .Select(i => new AutoSubscriberConsumerInfo(concreteType, i, i.GetGenericArguments()[0]))
                    .ToArray();

                if (subscriptionInfos.Any())
                {
                    yield return new KeyValuePair<Type, AutoSubscriberConsumerInfo[]>(concreteType, subscriptionInfos);
                }
            }
        }

        protected virtual IEnumerable<KeyValuePair<Type, AutoSubscriberResponderInfo[]>> GetResponderInfos(IEnumerable<Type> types, Type interfaceType)
        {
            foreach (var concreteType in types.Where(t => t.IsClass))
            {
                var subscriptionInfos = concreteType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                    .Select(i => new AutoSubscriberResponderInfo(concreteType, i, i.GetGenericArguments()[0], i.GetGenericArguments()[1]))
                    .ToArray();

                if (subscriptionInfos.Any())
                {
                    yield return new KeyValuePair<Type, AutoSubscriberResponderInfo[]>(concreteType, subscriptionInfos);
                }
            }
        }

        private MethodInfo ConsumeMethod(AutoSubscriberConsumerInfo consumerInfo)
        {
            return consumerInfo.ConcreteType.GetMethod(ConsumeMethodName, new[] { consumerInfo.MessageType }) ??
                   this.GetExplicitlyDeclaredInterfaceMethod(consumerInfo.MessageType);
        }

        private MethodInfo GetExplicitlyDeclaredInterfaceMethod(Type messageType)
        {
            var interfaceType = typeof(IConsume<>).MakeGenericType(messageType);
            return interfaceType.GetMethod(ConsumeMethodName);
        }
    }
}