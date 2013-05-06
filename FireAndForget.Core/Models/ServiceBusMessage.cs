
namespace FireAndForget.Core.Models
{
    public class ServiceBusMessage
    {
        public object Data { get; set; }

        public override string ToString()
        {
            return string.Format("Incoming message: {0}", Data);
        }
    }
}
