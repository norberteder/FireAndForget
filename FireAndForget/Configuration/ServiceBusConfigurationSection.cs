using System.Configuration;

namespace FireAndForget.Configuration
{
    public class ServiceBusConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("Server")]
        public ServerConfigurationElement Server
        {
            get { return (ServerConfigurationElement)this["Server"]; }
        }

        [ConfigurationProperty("Workers")]
        public WorkerConfigurationElementCollection Workers
        {
            get { return (WorkerConfigurationElementCollection)this["Workers"]; }
        }

        [ConfigurationProperty("Executors")]
        public ExecutorConfigurationElementCollection Executors
        {
            get { return (ExecutorConfigurationElementCollection)this["Executors"]; }
        }
    }
}
