using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics.CodeAnalysis;

namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceExtensions
    {
        public static IServiceCollection AddLogsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitialiser>();
            services.AddApplicationInsightsTelemetry(configuration.GetSection("APPINSIGHTS_CONNECTIONSTRING"));

            return services;
        }
    }
}
