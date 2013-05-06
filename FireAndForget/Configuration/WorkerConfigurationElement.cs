using System.Configuration;

namespace FireAndForget.Configuration
{
    public class WorkerConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }
    }
}
