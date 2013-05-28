using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FireAndForget.Core
{
    public class BusTaskDescriptor
    {
        public bool IsBulkTask { get; private set; }
        public bool IsDelayedTask { get; private set; }
        public DateTime? ExecuteAt { get; private set; }
        public string MessageType { get; private set; }

        public BusTaskDescriptor(string data)
        {
            var message = JObject.Parse(data);
            JToken executeAt;
            if (message.TryGetValue("ExecuteAt", out executeAt) && executeAt.Value<DateTime?>() != null)
            {
                IsDelayedTask = true;
                ExecuteAt = executeAt.Value<DateTime>();
            }

            MessageType = message.Value<string>("MessageType");
            IsBulkTask = message.Value<Boolean>("IsBulk");
        }

        public BusTaskDescriptor(BusTask task)
            : this(task.Data)
        {
            
        }
    }
}
