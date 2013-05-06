using System;
using System.Runtime.Serialization;

namespace FireAndForget.Exceptions
{
    public class ServiceBusInitializationException : Exception
    {
        public ServiceBusInitializationException() { }
        public ServiceBusInitializationException(string message) : base(message) { }
        public ServiceBusInitializationException(string message, Exception innerException) : base(message, innerException) { }
        public ServiceBusInitializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
