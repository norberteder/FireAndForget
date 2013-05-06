using System.Configuration;

namespace FireAndForget.Configuration
{
    public class ExecutorConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("worker", IsRequired = true)]
        public string Worker
        {
            get { return (string)this["worker"]; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string)this["type"]; }
        }
    }
}
