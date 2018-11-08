using Newtonsoft.Json;
using System.Web.Configuration;
using System.Web.Http;
using ApacKernel;
using ApacKernel.AspNet;
using ApacKernel.AspNet.Configuration;
using ApacKernel.AspNet.Security.Authentication.Systems;
using ApacKernel.AspNet.WebApi;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace IntegrationService.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.UseApiRoutePrefix(new ApiPrefixSettings { UsePrefix = false });
            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            config.Formatters.Clear();
            config.UseJsonNetFormatter(settings);
            config.UseServerStamp();
            config.EnsureInitialized();
            SetCustomErrors(config);

            var systemConfiguration = AspNetConfiguration.Instance.GetByRole("keyServiceConfig");
            SystemServices.Global.UseApiKeyServerAuthentication(new ApiKeyServerAuthenticationSettings(systemConfiguration, ApacConfig.AppSettings.SystemID));
        }
        private static void SetCustomErrors(HttpConfiguration config)
        {
            switch (WebConfig.CustomErrorsMode())
            {
                case CustomErrorsMode.RemoteOnly:
                    config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
                    break;
                case CustomErrorsMode.On:
                    config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
                    break;
                case CustomErrorsMode.Off:
                    config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                    break;
                default:
                    config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
                    break;
            }
        }
    }
}