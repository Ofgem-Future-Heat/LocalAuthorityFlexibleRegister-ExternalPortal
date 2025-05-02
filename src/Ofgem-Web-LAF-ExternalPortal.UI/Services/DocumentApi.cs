using Microsoft.Net.Http.Headers;

namespace Ofgem_Web_LAF_ExternalPortal.Services
{
    public static class DocumentApi
    {
        public const string ApiName = "LAF.Api.Document";
        public const string Route = "api/documents";
        public const string RouteHealthCheck = "api/health";
        public const string RouteHealthCheckFull = "api/healthfullcheck";

        public static void ConfigureHttpClient(WebApplicationBuilder builder)
        {
            var apiEndpoint = builder.Configuration["DocumentServiceApiUrl"] 
                              ?? throw new ArgumentNullException(nameof(builder), @"DocumentServiceApiUrl Api endpoint is not configured");

            builder.Services.AddHttpClient(ApiName, httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiEndpoint);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "HttpRequestsSample");
            }).AddHttpMessageHandler<HeaderHandler>();
        }
    }
}