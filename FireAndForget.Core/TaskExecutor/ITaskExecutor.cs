
namespace FireAndForget.Core.TaskExecutor
{
    /// <summary>
    /// Interface for all kinds of task executors
    /// </summary>
    public interface ITaskExecutor
    {
        /// <summary>
        /// Gets the message type of this task executor. The name will be matched to the task's name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string MessageType { get; }

        /// <summary>
        /// Executes the executor with the given data
        /// </summary>
        /// <param name="data">The data that's necessary for this executor</param>
        void Process(string data);
    }
}
