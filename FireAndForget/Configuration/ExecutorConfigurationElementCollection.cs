using System.Configuration;

namespace FireAndForget.Configuration
{
    [ConfigurationCollection(typeof(ExecutorConfigurationElement), AddItemName = "Executor")]
    public class ExecutorConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ExecutorConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element.GetHashCode();
        }

        public ExecutorConfigurationElement this[int idx] { get { return (ExecutorConfigurationElement)BaseGet(idx); } }
    }
}
