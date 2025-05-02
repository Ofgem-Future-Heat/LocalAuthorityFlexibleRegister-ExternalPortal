using Microsoft.Net.Http.Headers;

namespace Ofgem_Web_LAF_ExternalPortal.Services
{
    public static class UserApi
    {
        public const string ApiName = "LAF.Api.User";
        public const string Route = "api/User";
        public const string RouteGetByEmail = "api/User/email";
        public const string RoutePut = "api/User";

        public const string RouteHealthCheck = "api/health";
        public const string RouteHealthCheckFull = "api/healthfullcheck";

        public const string RouteAddAnnouncement = "/api/announcements/Add";
        public const string RouteEditAnnouncement = "/api/announcements/Edit";
        public const string RouteGetAllAnnouncements = "/api/announcements/GetAll";
        public const string RouteGetPublishedAnnouncements = "/api/announcements/GetPublished";
        public const string RouteGetAnnouncement = "/api/announcements";
        public const string RouteUnPublishAnnouncement = "/api/announcements/UnPublish";
        public const string RouteDeleteAnnouncement = "/api/announcements";

        private const string ExternalUserBaseRoute = "/api/external-user";

        public const string RouteAddExternalUser = ExternalUserBaseRoute;
        public const string RouteEditExternalUser = ExternalUserBaseRoute;
        public const string RouteDeactivateExternalUser = $"{ExternalUserBaseRoute}/de-activate";
        public const string RouteReactivateExternalUser = $"{ExternalUserBaseRoute}/re-activate";
        public const string RouteGetExternalUser = ExternalUserBaseRoute;
        public const string RouteGetFilteredExternalUser = $"{ExternalUserBaseRoute}/GetFiltered";
        public const string RouteExistingAuthorisedSignatory = "authorised-signatory";

        public static void ConfigureHttpClient(WebApplicationBuilder builder)
        {
            var apiEndpoint = builder.Configuration["UserServiceApiUrl"]
                              ?? throw new ArgumentNullException(nameof(builder), @"UserServiceApiUrl Api endpoint is not configured");

            builder.Services.AddHttpClient(ApiName, httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiEndpoint);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "HttpRequestsSample");
            }).AddHttpMessageHandler<HeaderHandler>();
        }
    }
}
