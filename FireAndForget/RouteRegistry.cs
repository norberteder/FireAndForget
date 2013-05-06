using System.Web.Http;

namespace FireAndForget
{
    internal static class RouteRegistry
    {
        internal static void Register(HttpRouteCollection routes)
        {
            routes.MapHttpRoute("PostMessage", "api/v1/enqueue/", new { controller = "servicebus" });
            routes.MapHttpRoute("RequeueErrors", "api/v1/retry/{queue}", new { controller = "retry", queue = RouteParameter.Optional });
        }
    }
}
