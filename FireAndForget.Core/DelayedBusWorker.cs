using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FireAndForget.Core.Persistence;
using FireAndForget.Core.TaskExecutor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FireAndForget.Core
{
    internal class DelayedBusWorker
    {
        private Bus bus;
        private Timer timer = new Timer(30000);
        private bool inProgress = false;

        public DelayedBusWorker(Bus bus)
        {
            this.bus = bus;
            timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!inProgress)
            {
                inProgress = true;

                var delayedTasks = DatabaseManager.Instance.GetAllOpenAndDelayedTasks().ToList();
                var tasksToHandle = GetTasksToExecute(delayedTasks);
                Dictionary<string, List<BusTask>> bulkTasks = new Dictionary<string, List<BusTask>>();

                foreach (var task in tasksToHandle)
                {
                    var taskDescriptor = new BusTaskDescriptor(task);
                    if (taskDescriptor.IsBulkTask)
                    {
                        if (!bulkTasks.ContainsKey(taskDescriptor.MessageType))
                            bulkTasks.Add(taskDescriptor.MessageType, new List<BusTask>());

                        bulkTasks[taskDescriptor.MessageType].Add(task);
                        continue;
                    }

                    this.bus.ScheduleImmediately(task);
                }

                if (bulkTasks.Count > 0)
                {
                    foreach (var messageType in bulkTasks.Keys)
                    {
                        var executor = bus.ResolveBulkExecutor(messageType);

                        var tasksForMessageType = bulkTasks[messageType];
                        var data = CreateMessageFromTasks(messageType, tasksForMessageType);
                        try
                        {
                            executor.Process(data.ToString());
                            foreach (var task in tasksForMessageType)
                            {
                                task.Finish();
                                DatabaseManager.Instance.Update(task);
                            }
                        }
                        catch (Exception ex)
                        {
                            foreach (var task in tasksForMessageType)
                            {
                                task.SetError(ex);
                                DatabaseManager.Instance.Update(task);
                            }
                        }
                    }
                }

                inProgress = false;
            }
        }

        private object CreateMessageFromTasks(string messageType, List<BusTask> tasksForMessageType)
        {
            var message = new TempMessage();
            message.MessageType = messageType;

            tasksForMessageType.ForEach(item => 
            {
                JObject data = JObject.Parse(item.Data);
                message.Data.Add(data.Value<string>("Data"));
            });

            return JsonConvert.SerializeObject(message);
        }

        private List<BusTask> GetTasksToExecute(List<BusTask> tasks)
        {
            return tasks.Where(item => item.ExecuteAt <= DateTime.Now).ToList();
        }

        internal void Start()
        {
            timer.Start();
        }

        class TempMessage
        {
            private List<string> data = new List<string>();

            public string MessageType { get; set; }
            public List<string> Data { get { return data; } }
        }
    }

}
