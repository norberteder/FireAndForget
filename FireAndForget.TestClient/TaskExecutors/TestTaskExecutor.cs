using System;
using System.Threading;
using FireAndForget.Core.TaskExecutor;
using Newtonsoft.Json.Linq;

namespace FireAndForget.TestClient.TaskExecutors
{
    public class TestTaskExecutor : ITaskExecutor
    {
        public string MessageType
        {
            get { return "TestTask"; }
        }

        public bool SupportsBulkTasks
        {
            get { return true; }
        }

        public void Process(string data)
        {
            JObject parsedData = JObject.Parse(data);
            JToken dataToken = parsedData["Data"];

            if (dataToken is JArray)
            {
                JArray items = (JArray)dataToken;
                foreach (var item in items)
                {
                    PrintMessageData(item, true);
                }
            }
            else
            {
                PrintMessageData(parsedData.Value<string>("Data"), false);
            }
        }

        private void PrintMessageData(object value, bool fromArray)
        {
            Console.WriteLine(value + " " + fromArray);
        }
    }
}
