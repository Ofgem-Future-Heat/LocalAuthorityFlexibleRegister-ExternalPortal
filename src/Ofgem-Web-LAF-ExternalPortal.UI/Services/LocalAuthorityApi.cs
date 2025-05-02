using Microsoft.Net.Http.Headers;

namespace Ofgem_Web_LAF_ExternalPortal.Services
{
    public static class LocalAuthorityApi
    {
        public const string ApiName = "LAF.Api.LocalAuthority";
        public const string Route = "api/LocalAuthorities";
        public const string RouteAdd = "api/LocalAuthorities";
        public const string RouteGetByOnsCode = "api/LocalAuthorities/by-ons-code/";
        public const string RouteOnsExists = "api/LocalAuthorities/by-ons-code/";
        public const string RouteNameExists = "api/LocalAuthorities/by-name/";
        public const string RoutePutByOnsCode = "api/LocalAuthorities/by-ons-code/";
        public const string RouteGetFiltered = "api/LocalAuthorities/GetFiltered";
        public const string RouteHealthCheck = "api/health";
        public const string RouteHealthCheckFull = "api/healthfullcheck";

        public const string RouteSoiList = "api/soi/soiList";
        public const string RouteSoiById = "api/soi/by-soi-id/";
        public const string RouteCreateSoi = "api/soi";
        public const string RoutePutUpdateSOISignOffChecklist = "api/soi/updateSignOffCheckList";
        public const string RoutePutUpdateSOIAssessmentChecklist = "api/soi/updateAssessmentCheckList";
        public const string RoutePutUpdateSOIRoutes = "api/soi/updateRoutes";
        public const string RoutePutUpdateSoi = "api/soi/updateStatementOfIntent";
        public const string RouteGetSoiByOns = "api/soi/GetSoiByOns";

        public const string RouteCreateAssessmentNote = "api/assessment-notes";
        public const string RouteDeleteAssessmentNote = "api/assessment-notes";

        public const string RouteCreateInternalNote = "api/internal-notes";
        public const string RouteDeleteInternalNote = "api/internal-notes";

        public static void ConfigureHttpClient(WebApplicationBuilder builder)
        {
            var apiEndpoint = builder.Configuration["LocalAuthorityServiceApiUrl"]
                              ?? throw new ArgumentNullException(nameof(builder), @"LocalAuthorityServiceApiUrl Api endpoint is not configured");

            builder.Services.AddHttpClient(ApiName, httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiEndpoint);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "HttpRequestsSample");
            }).AddHttpMessageHandler<HeaderHandler>();
        }
    }
}
