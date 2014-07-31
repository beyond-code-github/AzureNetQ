namespace AzureNetQ.NonGeneric
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using AzureNetQ.FluentConfiguration;

    public static class NonGenericExtensions
    {
        public static IDisposable Subscribe(this IBus bus, Type messageType, Action<object> onMessage)
        {
            return Subscribe(bus, messageType, onMessage, configuration => { });
        }

        public static IDisposable Subscribe(
            this IBus bus,
            Type messageType,
            Action<object> onMessage,
            Action<ISubscriptionConfiguration> configure)
        {
            Preconditions.CheckNotNull(onMessage, "onMessage");

            Func<object, Task> asyncOnMessage = x => TaskHelpers.ExecuteSynchronously(() => onMessage(x));

            return SubscribeAsync(bus, messageType, asyncOnMessage, configure);
        }

        public static IDisposable SubscribeAsync(
            this IBus bus,
            Type messageType,
            Func<object, Task> onMessage)
        {
            return SubscribeAsync(bus, messageType, onMessage, configuration => { });
        }

        public static IDisposable SubscribeAsync(
            this IBus bus,
            Type messageType,
            Func<object, Task> onMessage,
            Action<ISubscriptionConfiguration> configure)
        {
            Preconditions.CheckNotNull(bus, "bus");
            Preconditions.CheckNotNull(messageType, "messageType");
            Preconditions.CheckNotNull(onMessage, "onMessage");
            Preconditions.CheckNotNull(configure, "configure");

            var subscribeMethodOpen = typeof(IBus)
                .GetMethods()
                .SingleOrDefault(x => x.Name == "SubscribeAsync" && HasCorrectParameters(x));

            if (subscribeMethodOpen == null)
            {
                throw new AzureNetQException("API change? SubscribeAsync method not found on IBus");
            }

            var subscribeMethod = subscribeMethodOpen.MakeGenericMethod(messageType);
            return (IDisposable)subscribeMethod.Invoke(bus, new object[] { onMessage, configure });
        }
        
        private static bool HasCorrectParameters(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            return
                parameters.Length == 2 &&
                parameters[0].ParameterType.Name == "Func`2" &&
                parameters[1].ParameterType == typeof(Action<ISubscriptionConfiguration>);
        }
    }
}
