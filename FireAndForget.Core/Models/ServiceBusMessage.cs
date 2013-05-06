
namespace FireAndForget.Core.Models
{
    /// <summary>
    /// The data transfer object containing all data necessary for the message bus and configured exeuctors
    /// </summary>
    public class ServiceBusMessage
    {
        /// <summary>
        /// Gets or sets the data for the message bus and the configured executor
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public object Data { get; set; }

        public override string ToString()
        {
            return string.Format("Incoming message: {0}", Data);
        }
    }
}
