using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AzureNetQ.AutoSubscribe
{
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
        protected readonly IBus bus;
        
        /// <summary>
        /// Responsible for consuming a message with the relevant message consumer.
        /// </summary>
        public IAutoSubscriberMessageDispatcher AutoSubscriberMessageDispatcher { get; set; } 

        public AutoSubscriber(IBus bus)
        {
            Preconditions.CheckNotNull(bus, "bus");
           
            this.bus = bus;
            AutoSubscriberMessageDispatcher = new DefaultAutoSubscriberMessageDispatcher();
        }
        
        /// <summary>
        /// Registers all consumers in passed assembly. The actual Subscriber instances is
        /// created using <seealso cref="AutoSubscriberMessageDispatcher"/>.
        /// </summary>
        /// <param name="assemblies">The assembleis to scan for consumers.</param>
        public virtual void Subscribe(params Assembly[] assemblies)
        {
            Preconditions.CheckAny(assemblies, "assemblies", "No assemblies specified.");

            var genericBusSubscribeMethod = this.GetSubscribeMethodOfBus("Subscribe", typeof(Action<>));
            var subscriptionInfos = this.GetConsumerInfos(assemblies.SelectMany(a => a.GetTypes()), typeof(IConsume<>));
            
            InvokeMethods(
                subscriptionInfos,
                DispatchMethodName,
                genericBusSubscribeMethod,
                messageType => typeof(Action<>).MakeGenericType(messageType));

            var genericBusRespondMethod = this.GetRespondMethodOfBus("Respond", typeof(Func<,>));
            var responderInfos = this.GetResponderInfos(assemblies.SelectMany(a => a.GetTypes()), typeof(IRespond<,>));
            
            InvokeMethods(
                responderInfos,
                HandleMethodName,
                genericBusRespondMethod,
                (messageType, responseType) => typeof(Func<,>).MakeGenericType(messageType, responseType));

        }
        
        /// <summary>
        /// Registers all async consumers in passed assembly. The actual Subscriber instances is
        /// created using <seealso cref="AutoSubscriberMessageDispatcher"/>.
        /// </summary>
        /// <param name="assemblies">The assembleis to scan for consumers.</param>
        public virtual void SubscribeAsync(params Assembly[] assemblies)
        {
            Preconditions.CheckAny(assemblies, "assemblies", "No assemblies specified.");

            var genericBusSubscribeMethod = this.GetSubscribeMethodOfBus("SubscribeAsync", typeof(Func<,>));
            var consumerInfos = this.GetConsumerInfos(assemblies.SelectMany(a => a.GetTypes()), typeof(IConsumeAsync<>));
            Func<Type,Type> subscriberTypeFromMessageTypeDelegate = messageType => typeof (Func<,>).MakeGenericType(messageType, typeof (Task));
            
            InvokeMethods(
                consumerInfos,
                DispatchAsyncMethodName,
                genericBusSubscribeMethod,
                subscriberTypeFromMessageTypeDelegate);

            var genericBusRespondMethod = this.GetRespondMethodOfBus("RespondAsync", typeof(Func<,>));
            var responderInfos = this.GetResponderInfos(assemblies.SelectMany(a => a.GetTypes()), typeof(IRespondAsync<,>));
            Func<Type, Type, Type> subscriberTypeFromMessageAndResponseTypeDelegate =
                (messageType, responseType) => typeof(Func<,>).MakeGenericType(messageType, typeof(Task<>).MakeGenericType(responseType));

            InvokeMethods(
               responderInfos,
               HandleAsyncMethodName,
               genericBusRespondMethod,
               subscriberTypeFromMessageAndResponseTypeDelegate);
        }

        protected void InvokeMethods(IEnumerable<KeyValuePair<Type, AutoSubscriberConsumerInfo[]>> subscriptionInfos, string dispatchName, MethodInfo genericBusSubscribeMethod, Func<Type, Type> subscriberTypeFromMessageTypeDelegate)
        {
            foreach (var kv in subscriptionInfos)
            {
                foreach (var subscriptionInfo in kv.Value)
                {
                    var dispatchMethod =
                            AutoSubscriberMessageDispatcher.GetType()
                                                           .GetMethod(dispatchName, BindingFlags.Instance | BindingFlags.Public)
                                                           .MakeGenericMethod(subscriptionInfo.MessageType, subscriptionInfo.ConcreteType);

                    var configuration = new Action<ISubscriptionConfiguration>(c => { });
                    var subscriptionAttribute = GetSubscriptionAttribute(subscriptionInfo);

                    if (subscriptionAttribute != null)
                    {
                        configuration = c => c.WithSubscription(subscriptionAttribute.Name);
                    }
                    
                    var dispatchDelegate = Delegate.CreateDelegate(subscriberTypeFromMessageTypeDelegate(subscriptionInfo.MessageType), AutoSubscriberMessageDispatcher, dispatchMethod);
                    var busSubscribeMethod = genericBusSubscribeMethod.MakeGenericMethod(subscriptionInfo.MessageType);

                    busSubscribeMethod.Invoke(bus, new object[] { dispatchDelegate, configuration });
                }
            }
        }

        protected virtual SubscriptionAttribute GetSubscriptionAttribute(AutoSubscriberConsumerInfo subscriptionInfo)
        {
            var consumeMethod = ConsumeMethod(subscriptionInfo);
            return consumeMethod.GetCustomAttributes(typeof(SubscriptionAttribute), true).SingleOrDefault() as SubscriptionAttribute;
        }

        private MethodInfo ConsumeMethod(AutoSubscriberConsumerInfo consumerInfo)
        {
            return consumerInfo.ConcreteType.GetMethod(ConsumeMethodName, new[] { consumerInfo.MessageType }) ??
                   GetExplicitlyDeclaredInterfaceMethod(consumerInfo.MessageType);
        }

        private MethodInfo GetExplicitlyDeclaredInterfaceMethod(Type messageType)
        {
            var interfaceType = typeof(IConsume<>).MakeGenericType(messageType);
            return interfaceType.GetMethod(ConsumeMethodName);
        }

        protected void InvokeMethods(IEnumerable<KeyValuePair<Type, AutoSubscriberResponderInfo[]>> subscriptionInfos, string handlerName, MethodInfo genericBusRespondMethod, Func<Type, Type, Type> subscriberTypeFromMessageTypeDelegate)
        {
            foreach (var kv in subscriptionInfos)
            {
                foreach (var subscriptionInfo in kv.Value)
                {
                    var handleMethod =
                            AutoSubscriberMessageDispatcher.GetType()
                                                           .GetMethod(handlerName, BindingFlags.Instance | BindingFlags.Public)
                                                           .MakeGenericMethod(subscriptionInfo.MessageType, subscriptionInfo.ResponseType, subscriptionInfo.ConcreteType);

                    var handleDelegate = Delegate.CreateDelegate(subscriberTypeFromMessageTypeDelegate(subscriptionInfo.MessageType, subscriptionInfo.ResponseType), AutoSubscriberMessageDispatcher, handleMethod);
                    var busRespondMethod = genericBusRespondMethod.MakeGenericMethod(subscriptionInfo.MessageType, subscriptionInfo.ResponseType);

                    busRespondMethod.Invoke(bus, new object[] { handleDelegate });
                }
            }
        }
        
        protected virtual bool IsValidMarkerType(Type markerType)
        {
            return markerType.IsInterface && markerType.GetMethods().Any(m => m.Name == ConsumeMethodName);
        }

        protected virtual MethodInfo GetSubscribeMethodOfBus(string method, Type paramType)
        {
            return bus.GetType().GetMethods()
                .Where(m => m.Name == method)
                .Select(m => new { Method = m, Params = m.GetParameters() })
                .Single(m => m.Params.Length == 2
                    && m.Params[0].ParameterType.GetGenericTypeDefinition() == paramType
                    && m.Params[1].ParameterType == typeof(Action<ISubscriptionConfiguration>)
                   ).Method;
        }

        protected virtual MethodInfo GetRespondMethodOfBus(string method, Type paramType)
        {
            return bus.GetType().GetMethods()
                .Where(m => m.Name == method)
                .Select(m => new { Method = m, Params = m.GetParameters() })
                .Single(m => m.Params.Length == 1
                    && m.Params[0].ParameterType.GetGenericTypeDefinition() == paramType
                   ).Method;
        }
        
        protected virtual IEnumerable<KeyValuePair<Type, AutoSubscriberConsumerInfo[]>> GetConsumerInfos(IEnumerable<Type> types,Type interfaceType)
        {
            foreach (var concreteType in types.Where(t => t.IsClass))
            {
                var subscriptionInfos = concreteType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                    .Select(i => new AutoSubscriberConsumerInfo(concreteType, i, i.GetGenericArguments()[0]))
                    .ToArray();

                if (subscriptionInfos.Any())
                    yield return new KeyValuePair<Type, AutoSubscriberConsumerInfo[]>(concreteType, subscriptionInfos);
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
                    yield return new KeyValuePair<Type, AutoSubscriberResponderInfo[]>(concreteType, subscriptionInfos);
            }
        }
    }
}