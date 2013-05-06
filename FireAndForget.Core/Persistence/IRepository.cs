using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireAndForget.Core.Persistence
{
    /// <summary>
    /// Interface for implementing data access
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Adds the specified task
        /// </summary>
        /// <param name="task">The task to be persisted</param>
        void Add(BusTask task);
        /// <summary>
        /// Updates the specified task.
        /// </summary>
        /// <param name="task">The task to be updated</param>
        void Update(BusTask task);
        /// <summary>
        /// Gets all open tasks.
        /// </summary>
        /// <returns>Returns an <see cref="IEnumerable"/> of <see cref="BusTask"/> representing open tasks</returns>
        IEnumerable<BusTask> GetAllOpenTasks();
        /// <summary>
        /// Gets all open tasks for queue.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns>Returns an <see cref="IEnumerable"/> of <see cref="BusTask"/> representing open tasks for the given queue</returns>
        IEnumerable<BusTask> GetAllOpenTasksForQueue(string queueName);
    }
}
