using System.Diagnostics.CodeAnalysis;

namespace Ofgem_Web_LAF_ExternalPortal.Services
{
    [ExcludeFromCodeCoverage]
    public static class FeatureService
    {
        //public static bool SingleDeclarationActive { get; set; }

        public static void ConfigureFeatureService(WebApplicationBuilder builder)
        {
            //var abc = builder.Configuration["FeatureSwitchSingleDeclarationActive"]
            //          ?? throw new ArgumentNullException(nameof(builder), @"FeatureSwitchSingleDeclarationActive is not configured");

            //if (bool.TryParse(abc, out bool result))
            //{
            //    SingleDeclarationActive = result;
            //}
            //else
            //{
            //    SingleDeclarationActive = false;
            //}
        }
    }
}
