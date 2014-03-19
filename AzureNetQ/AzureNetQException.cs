using System;
using System.Runtime.Serialization;

namespace AzureNetQ
{
    [Serializable]
    public class AzureNetQException : Exception
    {
        public AzureNetQException() {}
        public AzureNetQException(string message) : base(message) {}
        public AzureNetQException(string format, params string[] args) : base(string.Format(format, args)) {}
        public AzureNetQException(string message, Exception inner) : base(message, inner) {}

        protected AzureNetQException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }

    [Serializable]
    public class AzureNetQInvalidMessageTypeException : AzureNetQException
    {
        public AzureNetQInvalidMessageTypeException() {}
        public AzureNetQInvalidMessageTypeException(string message) : base(message) {}
        public AzureNetQInvalidMessageTypeException(string format, params string[] args) : base(format, args) {}
        public AzureNetQInvalidMessageTypeException(string message, Exception inner) : base(message, inner) {}
        protected AzureNetQInvalidMessageTypeException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }

    [Serializable]
    public class AzureNetQResponderException : AzureNetQException
    {
        public AzureNetQResponderException() { }
        public AzureNetQResponderException(string message) : base(message) { }
        public AzureNetQResponderException(string format, params string[] args) : base(format, args) { }
        public AzureNetQResponderException(string message, Exception inner) : base(message, inner) { }
        protected AzureNetQResponderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}