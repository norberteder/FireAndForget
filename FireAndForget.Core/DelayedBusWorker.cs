using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FireAndForget.Core.Persistence;
using Newtonsoft.Json.Linq;

namespace FireAndForget.Core
{
    internal class DelayedBusWorker
    {
        private Timer timer = new Timer(60000);
        private bool inProgress = false;

        public DelayedBusWorker()
        {
            timer.Elapsed += timer_Elapsed;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!inProgress)
            {
                inProgress = true;

                var delayedTasks = DatabaseManager.Instance.GetAllOpenAndDelayedTasks().ToList();
                var tasksToHandle = GetTasksToExecute(delayedTasks);

            }
        }

        private List<BusTask> GetTasksToExecute(List<BusTask> tasks)
        {
            return tasks.Where(item => item.ExecuteAt <= DateTime.Now).ToList();
        }
    }
}
