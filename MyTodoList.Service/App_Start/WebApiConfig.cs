
using System.Web.Http;

namespace MyTodoList.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Suppress authentication have been done by the host (IIS)
            // We will expect OpenID Connect identity_token in Authorization header
            config.SuppressHostPrincipal();

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
