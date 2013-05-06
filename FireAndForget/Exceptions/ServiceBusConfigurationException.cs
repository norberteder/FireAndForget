using System;
using System.Runtime.Serialization;

namespace FireAndForget.Exceptions
{
    public class ServiceBusConfigurationException : Exception
    {
        public ServiceBusConfigurationException() { }
        public ServiceBusConfigurationException(string message) : base(message) { }
        public ServiceBusConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        public ServiceBusConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
