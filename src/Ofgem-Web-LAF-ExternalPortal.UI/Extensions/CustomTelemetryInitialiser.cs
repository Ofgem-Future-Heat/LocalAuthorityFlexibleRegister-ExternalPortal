using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics.CodeAnalysis;

namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    [ExcludeFromCodeCoverage]
    public class CustomTelemetryInitialiser : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null) return;
            telemetry.Context.Cloud.RoleName = "LAF-ExternalPortal-Web";
        }
    }
}
