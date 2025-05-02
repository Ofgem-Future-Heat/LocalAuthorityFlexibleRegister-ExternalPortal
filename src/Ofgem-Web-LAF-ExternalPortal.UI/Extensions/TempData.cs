
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    public static class TempDataKeys
    {
        public static string DuplicateData { get; } = "DuplicateData";
        public static string SubmissionNote { get; } = "SubmissionNote";
        public static string DashboardFilterData { get; } = "DashboardFilterData";
        public static string DashboardDeclarationCount { get; } = "DashboardDeclarationCount";
        public static string StatementOfIntentFilterData { get; } = "StatementOfIntentFilterData";
        public static string StatementOfIntentCount { get; } = "StatementOfIntentCount";
        public static string StatementOfIntentData { get; } = "StatementOfIntentData";
        public static string SingleDeclarationData { get; } = "SingleDeclarationData";
    }

    public static class TempData
    {
        private static readonly System.Text.Json.JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };

        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class?
        {

            tempData[key] = System.Text.Json.JsonSerializer.Serialize(value);


            tempData.Keep(key);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class?
        {
            object? data = tempData.Peek(key);
#pragma warning disable CS8603 // Possible null reference return.
            return data == null ? null : System.Text.Json.JsonSerializer.Deserialize<T>((string)data, Options);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
