using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using FireAndForget.Core.Persistence;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FireAndForget.Core
{
    internal class DelayedBusWorker
    {
        private Bus bus;
        private Timer timer = new Timer(30000);
        private int inProgress = 0;

        public DelayedBusWorker(Bus bus)
        {
            this.bus = bus;
            timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (System.Threading.Interlocked.CompareExchange(ref inProgress, 1, 0) == 0) 
            {

                try
                {
                    timer.Stop();

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
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref inProgress, 0);
                    timer.Start();
                }
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
