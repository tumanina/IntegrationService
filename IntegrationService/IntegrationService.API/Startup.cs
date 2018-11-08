using System.Net;
using System.Web.Http;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(IntegrationService.API.Startup))]
namespace IntegrationService.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            LoggerConfig.Configure();
            var httpConfiguration = new HttpConfiguration();
            AutofacConfig.Register(app, httpConfiguration);
            AutomapperConfig.Register();
            WebApiConfig.Register(httpConfiguration);
            app.UseWebApi(httpConfiguration);
        }
    }
}
