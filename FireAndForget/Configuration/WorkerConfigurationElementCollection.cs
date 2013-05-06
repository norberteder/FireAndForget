using System.Configuration;

namespace FireAndForget.Configuration
{
    [ConfigurationCollection(typeof(WorkerConfigurationElement), AddItemName = "Worker")]
    public class WorkerConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WorkerConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WorkerConfigurationElement)(element)).Name;
        }

        public WorkerConfigurationElement this[int idx] { get { return (WorkerConfigurationElement)BaseGet(idx); } }
    }
}
