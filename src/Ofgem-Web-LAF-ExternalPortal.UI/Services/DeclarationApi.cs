using Microsoft.Net.Http.Headers;

namespace Ofgem_Web_LAF_ExternalPortal.Services
{
    public static class DeclarationApi
    {
        public const string ApiName = "LAF.Api.Declaration";

        public const string RouteCreateUpload = "api/upload/create";

        public const string Route = "api/declarations";

        public const string RouteGetFiltered = "api/declarations/GetFiltered";

        public const string RouteGetDownloadData = "api/declarations/GetDownloadData";

        public const string RouteValidation = "api/declarations/validate";
        public const string RouteValidationAcceptAll = "api/declarations/validate-super";
        public const string RouteValidationIgnoreDuplicates = "/api/declarations/validate-non-dupe";

        public const string RouteHealthCheck = "api/health";
        public const string RouteHealthCheckFull = "api/healthfullcheck";

        public const string RouteUploadCancel = "/api/upload/cancel";
        public const string RouteUploadList = "GetFiltered";
        public const string RouteUploadsProcessing = "UploadsProcessing";
        public const string RouteUploadsAwaitingDecision = "UploadsAwaitingDecision";

        public const string RouteUploadDownloadErrors = "/api/declarations/GetDeclarationErrorsByUploadId";

        public const string RouteCreateDeclarationNote = "/api/declaration-notes";
        public const string RouteGetDeclarationNote = "/api/declaration-notes";
        public const string RouteDeleteDeclarationNote = "/api/declaration-notes";

        public const string RouteAllSupersededDeclarations = "/api/declarations/GetAllSupersededDeclarationsByUrn";
        public const string RouteSupersededDeclarationDetail = "/api/declarations/GetSupersededDeclarationByUrn";

        public const string RouteUploadDeleteAll = "/api/upload-delete-all";

        public const string RouteRunCoreRules = "/api/declarations/run-core-rules";

        public const string RouteSaveEditedDeclaration = "/api/declarations/validate-raw";

        public const string RouteProfileSoiRerunRules = "/api/declarations/rerunrules";

        public const string RouteIsUrnExists = "/api/declarations/urn";

        public static void ConfigureHttpClient(WebApplicationBuilder builder)
        {
            var apiEndpoint = builder.Configuration["DeclarationServiceApiUrl"]
                              ?? throw new ArgumentNullException(nameof(builder), @"DeclarationServiceApiUrl Api endpoint is not configured");

            builder.Services.AddHttpClient(ApiName, httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiEndpoint);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "HttpRequestsSample");
            }).AddHttpMessageHandler<HeaderHandler>();
        }
    }
}
