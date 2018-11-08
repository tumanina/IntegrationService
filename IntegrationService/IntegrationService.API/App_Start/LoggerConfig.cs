using ApacKernel.Diagnostics;
using ApacKernel.Diagnostics.NewRelic;

namespace IntegrationService.API
{
    internal class LoggerConfig
    {
        public static void Configure()
        {
            Log.LoggerProvider = LoggerNewRelic.Provider;
        }
    }
}