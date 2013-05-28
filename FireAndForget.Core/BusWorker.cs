using System;
using System.Timers;
using FireAndForget.Core.Persistence;
using FireAndForget.Core.TaskExecutor;

namespace FireAndForget.Core
{
    internal class BusWorker
    {
        private Bus Bus { get; set; }        
        private Timer timer = new Timer(200);
        private bool taskInProgress = false;

        /// <summary>
        /// Gets the name of this worker
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        internal string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusWorker" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="bus">The bus.</param>
        internal BusWorker(string name, Bus bus)
        {
            Bus = bus;
            Name = name;

            timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (taskInProgress)
                return;

            timer.Stop();

            taskInProgress = true;

            BusTask task = Bus.Get(Name);

            while (task != null)
            {
                try
                {
                    ITaskExecutor executor = this.Bus.ResolveExecutor(task.MessageType);

                    task.Start();
                    DatabaseManager.Instance.Update(task);

                    executor.Process(task.Data);

                    task.Finish();
                    DatabaseManager.Instance.Update(task);
                }
                catch (Exception ex)
                {
                    task.SetError(ex);
                    DatabaseManager.Instance.Update(task);

                    Bus.AddFailed(task);
                }

                task = Bus.Get(Name);
            }

            taskInProgress = false;

            timer.Start();
        }

        /// <summary>
        /// Starts the worker.
        /// </summary>
        internal void Start()
        {
            timer.Start();
        }
    }
}
