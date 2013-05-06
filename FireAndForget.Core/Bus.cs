using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FireAndForget.Core.TaskExecutor;
using FireAndForget.Core.Persistence;

namespace FireAndForget.Core
{
    public class Bus
    {
        private ConcurrentDictionary<string, ConcurrentQueue<BusTask>> workingQueue = new ConcurrentDictionary<string, ConcurrentQueue<BusTask>>();
        private ConcurrentDictionary<string, ConcurrentQueue<BusTask>> errorQueue = new ConcurrentDictionary<string, ConcurrentQueue<BusTask>>();
        private ConcurrentDictionary<string, string> messageMapping = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, Dictionary<string, Type>> executors = new ConcurrentDictionary<string, Dictionary<string, Type>>();

        /// <summary>
        /// Registers a queue
        /// </summary>
        /// <param name="name">The name of the queue</param>
        public void RegisterQueue(string name)
        {
            workingQueue.TryAdd(name, new ConcurrentQueue<BusTask>());
            errorQueue.TryAdd(name, new ConcurrentQueue<BusTask>());
        }

        /// <summary>
        /// Schedules a new job
        /// </summary>
        /// <param name="data">The data to processed by the task</param>
        /// <exception cref="System.InvalidOperationException">queue does not exist</exception>
        public void Schedule(string data)
        {
            var messageType = RetrieveMessageType(data);

            var queue = ResolveQueueForExecutor(messageType);

            if (!workingQueue.ContainsKey(queue))
                throw new InvalidOperationException("queue does not exist");

            var task = new BusTask(queue, messageType, data);

            DatabaseManager.Instance.Add(task);

            workingQueue[queue].Enqueue(task);
        }

        public void Schedule(object data)
        {
            Schedule(data.ToString());
        }

        public void Schedule(BusTask task)
        {
            workingQueue[task.Queue].Enqueue(task);
        }

        private string RetrieveMessageType(string data)
        {
            var message = JObject.Parse(data);
            return message.Value<string>("MessageType");            
        }

        /// <summary>
        /// Registers an executor
        /// </summary>
        /// <param name="worker">Name of the worker to execute this task</param>
        /// <param name="executorType">Type of the executor to be registered</param>
        public void RegisterExecutor(string worker, Type executorType)
        {
            var instance = Activator.CreateInstance(executorType);
            var executor = instance as ITaskExecutor;
            if (executor != null)
            {
                if (!executors.ContainsKey(worker))
                {
                    // TODO: error handling
                    Dictionary<string, Type> mapping = new Dictionary<string, Type>();
                    mapping.Add(executor.MessageType, executorType);
                    executors.TryAdd(worker, mapping);
                }
                else if (!executors[worker].ContainsKey(executor.MessageType))
                {
                    executors[worker].Add(executor.MessageType, executorType);
                }
            }
        }

        /// <summary>
        /// Resolves an executor by a given name
        /// </summary>
        /// <param name="messageType">The name of the executor to be resolved</param>
        /// <returns>the type of a found executor or <see cref="null"/></returns>
        public Type ResolveExecutor(string messageType)
        {
            var keys = executors.Keys;
            foreach (var key in keys)
            {
                if (executors[key].ContainsKey(messageType))
                    return executors[key][messageType];
            }
            return null;
        }

        /// <summary>
        /// Resolves the queue for the given message type
        /// </summary>
        /// <param name="messageType">The type of the message</param>
        /// <returns>The name of the queue or <see cref="null"/> if no queue for this message type exists</returns>
        public string ResolveQueueForExecutor(string messageType)
        {
            var keys = executors.Keys;
            foreach (var key in keys)
            {
                if (executors[key].ContainsKey(messageType))
                    return key;
            }
            return null;
        }

        /// <summary>
        /// Adds a task to the error queue
        /// </summary>
        /// <param name="task">The task.</param>
        public void AddFailed(BusTask task)
        {
            errorQueue[task.Queue].Enqueue(task);
        }

        /// <summary>
        /// Gets the next item from a specific queue
        /// </summary>
        /// <param name="queue">The queue to be dequeued</param>
        /// <returns>a <see cref="BusTask"/></returns>
        /// <exception cref="System.InvalidOperationException">queue does not exist</exception>
        public BusTask Get(string queue)
        {
            if (!workingQueue.ContainsKey(queue))
                throw new InvalidOperationException("queue does not exist");

            BusTask task = null;
            if (workingQueue[queue].TryDequeue(out task))
            {
                return task;
            }
            return null;
        }
    }
}
