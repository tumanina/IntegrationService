using AutoMapper;
using IntegrationService.API.Areas.V1;

namespace IntegrationService.API
{
    public class AutomapperConfig
    {
        public static void Register()
        {
            ConfigureMapping();
        }
        private static void ConfigureMapping()
        {
            // add automapper profiles' names here
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
        }
    }
}