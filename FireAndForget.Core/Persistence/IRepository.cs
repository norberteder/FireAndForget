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
        void Add(BusTask task);
        void Update(BusTask task);
        IEnumerable<BusTask> GetAllOpenTasks();
        IEnumerable<BusTask> GetAllOpenTasksForQueue(string queueName);
    }
}
