using System.Net;
using System.Net.Http;
using System.Web.Http;
using FireAndForget.Core;
using FireAndForget.Core.Models;

namespace FireAndForget.Controllers
{
    public class ServiceBusController : ApiController
    {
        public ServiceBusMessage Post(ServiceBusMessage message)
        {
            if (message == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

            lock (this)
            {
                try
                {
                    BusManager.Instance.Enqueue(message);
                }
                catch (NoConfiguredBusException ncbe)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable));
                }
            }

            return message;
        }
    }
}
