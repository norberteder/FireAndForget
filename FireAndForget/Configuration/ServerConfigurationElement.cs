using System;
using System.Configuration;

namespace FireAndForget.Configuration
{
    public class ServerConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("uri", IsRequired = true)]
        public Uri Uri
        {
            get { return new Uri(this["uri"].ToString()); }
        }
    }
}
