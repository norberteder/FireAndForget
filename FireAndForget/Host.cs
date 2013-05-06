using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace FireAndForget
{
    class Host
    {
        private Uri Uri { get; set; }

        public Host(Uri uri)
        {
            Uri = uri;
        }

        public void Run()
        {
            var config = new HttpSelfHostConfiguration(Uri);
            config.HostNameComparisonMode = HostNameComparisonMode.Exact;

            RouteRegistry.Register(config.Routes);

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                System.Console.WriteLine("Press Enter to quit.");
                System.Console.ReadLine();
            }
        }
    }
}
