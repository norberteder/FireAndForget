using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FireAndForget.Core;

namespace FireAndForget.Controllers
{
    public class RetryController : ApiController
    {
        public void Retry()
        {
            BusManager.Instance.RetryErroneousTasks();            
        }

        public void Retry(string queue)
        {
            BusManager.Instance.RetryErroneousTasks(queue);
        }
    }
}
