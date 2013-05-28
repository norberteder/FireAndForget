using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FireAndForget.Core.TaskExecutor;
using FireAndForget.Core.Persistence;

namespace FireAndForget.Core
{
    internal class Bus
    {
        private ConcurrentDictionary<string, ConcurrentQueue<BusTask>> workingQueue = new ConcurrentDictionary<string, ConcurrentQueue<BusTask>>();
        private ConcurrentDictionary<string, ConcurrentQueue<BusTask>> errorQueue = new ConcurrentDictionary<string, ConcurrentQueue<BusTask>>();
        private ConcurrentDictionary<string, string> messageMapping = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, List<KeyValuePair<string, Type>>> executors = new ConcurrentDictionary<string, List<KeyValuePair<string, Type>>>();

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
        /// Schedules the given object. 
        /// </summary>
        /// <param name="data">An <see cref="object"/> that can be deserialized by the message bus</param>
        public void Schedule(object data)
        {
            Schedule(data.ToString());
        }

        /// <summary>
        /// Schedules a new job
        /// </summary>
        /// <param name="data">The data to processed by the task</param>
        /// <exception cref="System.InvalidOperationException">queue does not exist</exception>
        public void Schedule(string data)
        {
            var taskDescriptor = new BusTaskDescriptor(data);
            var messageType = taskDescriptor.MessageType;

            var queue = ResolveQueueForExecutor(messageType);

            if (!workingQueue.ContainsKey(queue))
                throw new InvalidOperationException("queue does not exist");

            var task = new BusTask(queue, messageType, data);

            if (taskDescriptor.IsDelayedTask)
            {
                task.Delayed(taskDescriptor.ExecuteAt.Value);
            }
            
            DatabaseManager.Instance.Add(task);

            if (!taskDescriptor.IsDelayedTask)
            {
                workingQueue[queue].Enqueue(task);
            }
        }

        /// <summary>
        /// Schedules an instance of <see cref="BusTask"/>
        /// </summary>
        /// <param name="task">The task.</param>
        public void Schedule(BusTask task)
        {
            var taskDescriptor = new BusTaskDescriptor(task);
            if (!taskDescriptor.IsDelayedTask)
            {
                workingQueue[task.Queue].Enqueue(task);
            }
        }

        public void ScheduleImmediately(BusTask task)
        {
            workingQueue[task.Queue].Enqueue(task);
        }

        /// <summary>
        /// Reenqueues all erroneous tasks
        /// </summary>
        public void RetryErroneousTasks()
        {
            foreach (string queue in errorQueue.Keys)
            {
                RetryErroneousTasks(queue);
            }
        }

        /// <summary>
        /// Reenqueues all erroneous tasks for a specific queue
        /// </summary>
        /// <param name="queue">The name of the queue.</param>
        public void RetryErroneousTasks(string queue)
        {
            BusTask task = GetErrorTask(queue);
            while (task != null)
            {
                Schedule(task);
                task = GetErrorTask(queue);
            }
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
                    List<KeyValuePair<string, Type>> mapping = new List<KeyValuePair<string, Type>>();
                    mapping.Add(new KeyValuePair<string,Type>(executor.MessageType, executorType));
                    executors.TryAdd(worker, mapping);
                }
                else //if (!executors[worker].ContainsKey(executor.MessageType))
                {
                    var existingIndex = executors[worker].FindIndex(item => item.Key == executor.MessageType);
                    if (existingIndex < 0)
                        executors[worker].Add(new KeyValuePair<string, Type>(executor.MessageType, executorType));
                }
            }
        }

        /// <summary>
        /// Resolves an executor by a given name
        /// </summary>
        /// <param name="messageType">The name of the executor to be resolved</param>
        /// <returns>an instance of a found executor or <see cref="null"/></returns>
        public ITaskExecutor ResolveExecutor(string messageType)
        {
            var keys = executors.Keys;
            foreach (var key in keys)
            {
                var index = executors[key].FindIndex(item => item.Key == messageType);
                if (index >= 0)
                {
                    Type type = executors[key].Find(item => item.Key == messageType).Value;
                    return (ITaskExecutor)Activator.CreateInstance(type);
                }
            }
            return null;
        }

        public ITaskExecutor ResolveBulkExecutor(string messageType)
        {
            var keys = executors.Keys;
            foreach (var key in keys)
            {
                var index = executors[key].FindIndex(item => item.Key == messageType);
                if (index >= 0)
                {
                    Type type = executors[key].Find(item => item.Key == messageType).Value;
                    var executor = (ITaskExecutor)Activator.CreateInstance(type);
                    if (executor.SupportsBulkTasks)
                        return executor;
                }
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
                if (executors[key].FindIndex(item => item.Key == messageType) >= 0)
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
                throw new InvalidOperationException("Queue '" + queue + "' does not exist");

            BusTask task = null;
            if (workingQueue[queue].TryDequeue(out task))
            {
                return task;
            }
            return null;
        }

        private BusTask GetErrorTask(string queue)
        {
            if (!errorQueue.ContainsKey(queue))
                throw new InvalidOperationException("Queue '" + queue + "' does not exist");

            BusTask task = null;
            if (errorQueue[queue].TryDequeue(out task))
            {
                return task;
            }
            return null;
        }
    }
}
