using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FireAndForget.Core;

namespace FireAndForget.Controllers
{
    public class RetryController : ApiController
    {
        public HttpResponseMessage Retry()
        {
            return Retry(null);
        }

        public HttpResponseMessage Retry(string queue)
        {
            try
            {
                if (string.IsNullOrEmpty(queue))
                {
                    lock (this)
                    {
                        BusManager.Instance.RetryErroneousTasks();
                    }
                }
                else
                {
                    lock (this)
                    {
                        BusManager.Instance.RetryErroneousTasks(queue);
                    }
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
