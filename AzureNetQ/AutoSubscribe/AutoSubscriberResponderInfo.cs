namespace AzureNetQ.AutoSubscribe
{
    using System;

    [Serializable]
    public class AutoSubscriberResponderInfo
    {
        public readonly Type ConcreteType;

        public readonly Type InterfaceType;

        public readonly Type MessageType;

        public readonly Type ResponseType;

        public AutoSubscriberResponderInfo(Type concreteType, Type interfaceType, Type messageType, Type responseType)
        {
            Preconditions.CheckNotNull(concreteType, "concreteType");
            Preconditions.CheckNotNull(interfaceType, "interfaceType");
            Preconditions.CheckNotNull(messageType, "messageType");
            Preconditions.CheckNotNull(responseType, "responseType");

            this.ConcreteType = concreteType;
            this.InterfaceType = interfaceType;
            this.MessageType = messageType;
            this.ResponseType = responseType;
        }
    }
}