using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireAndForget.Core.Models;
using FireAndForget.Core.Persistence;

namespace FireAndForget.Core
{
    public class BusManager
    {
        private static BusManager instance = new BusManager();
        private Bus bus = new Bus();
        private List<BusWorker> workers = new List<BusWorker>();

        private BusManager() { }

        /// <summary>
        /// Gets the instance of the BusManager
        /// </summary>
        /// <value>
        /// There exists only one single instance; That's what returns.
        /// </value>
        public static BusManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Registers a new worker by a given thread. Each worker runs within its own thread.
        /// A new queue for the worker will also be registered.
        /// </summary>
        /// <param name="name">The name of the queue</param>
        public void RegisterWorker(string name)
        {
            if (workers.Count(item => item.Name == name) == 0)
            {
                var worker = new BusWorker(name, bus);

                Task.Factory.StartNew(() => worker.Start());

                bus.RegisterQueue(name);

                var tasksForQueue = DatabaseManager.Instance.GetAllOpenTasksForQueue(name);
                foreach (var task in tasksForQueue)
                {
                    bus.Schedule(task);
                }

                workers.Add(worker);
            }
        }

        /// <summary>
        /// Registers an executor
        /// </summary>
        /// <param name="worker">Name of the worker to execute this task</param>
        /// <param name="executorType">Type of the executor to be registered</param>
        public void RegisterExecutor(string worker, Type executorType)
        {
            this.bus.RegisterExecutor(worker, executorType);
        }

        /// <summary>
        /// Enqueues a new message
        /// </summary>
        /// <param name="message">An instance of <see cref="ServiceBusMessage"/> describing the message itself.</param>
        public void Enqueue(ServiceBusMessage message)
        {
            bus.Schedule(message.Data);
        }

        /// <summary>
        /// Reenqueues all erroneous tasks
        /// </summary>
        public void RetryErroneousTasks()
        {
            bus.RetryErroneousTasks();
        }

        /// <summary>
        /// Reenqueues all erroneous tasks for a specific queue
        /// </summary>
        /// <param name="queue">The name of the queue.</param>
        public void RetryErroneousTasks(string queue)
        {
            bus.RetryErroneousTasks(queue);
        }
    }
}
