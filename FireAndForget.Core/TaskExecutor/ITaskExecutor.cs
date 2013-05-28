
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
        /// Gets a value indicating whether this task executor supports the execution of bulk tasks or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports bulk tasks]; otherwise, <c>false</c>.
        /// </value>
        bool SupportsBulkTasks { get; }

        /// <summary>
        /// Executes the executor with the given data
        /// </summary>
        /// <param name="data">The data that's necessary for this executor</param>
        void Process(string data);
    }
}
