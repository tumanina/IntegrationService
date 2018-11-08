using System.Configuration;
using System.Web.Configuration;
using ApacKernel;

namespace IntegrationService.API
{
    public static class WebConfig
    {
        public static string IntegrationServiceDb(this AppSettings settings)
        {
            return ApacConfig.ConnectionStrings.GetString("IntegrationServiceDb");
        }
        public static CustomErrorsMode CustomErrorsMode()
        {
            var config =
                (CustomErrorsSection)ConfigurationManager.GetSection("system.web/customErrors");
            return config.Mode;
        }

        public static string EventHubConnectionString(this ConnectionStrings connStrings)
        {
            return connStrings.GetString("EventHubConnectionString");
        }
    }
}