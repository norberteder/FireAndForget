using System;

namespace FireAndForget.Core
{
    public class NoConfiguredBusException : Exception
    {
        public NoConfiguredBusException(string message)
            : base(message)
        {

        }

        public NoConfiguredBusException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
