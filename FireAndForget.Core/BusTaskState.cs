
namespace FireAndForget.Core
{
    /// <summary>
    /// Describes the state of a <see cref="BusTask"/>
    /// </summary>
    public enum BusTaskState
    {
        /// <summary>
        /// Task has been enqueued but wasn't executed by now
        /// </summary>
        NotStarted = 0,
        /// <summary>
        /// Task has been started but not finished yet
        /// </summary>
        Started = 1,
        /// <summary>
        /// The task was finished
        /// </summary>
        Finished = 2,
        /// <summary>
        /// The task is in an error state
        /// </summary>
        Error = 9
    }
}
