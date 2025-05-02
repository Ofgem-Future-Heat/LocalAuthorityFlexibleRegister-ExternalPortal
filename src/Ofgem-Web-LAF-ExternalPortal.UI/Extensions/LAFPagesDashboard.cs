#pragma warning disable CA1707
namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {

        public static class Dashboard
        {
            public const string ROUTE = "/Dashboard";
            public const string METHOD_APPLY_FILTERS = "ApplyFilters";
            public const string METHOD_CLEAR_FILTERS = "ClearFilters";
            public const string METHOD_DOWNLOAD_SELECTED = "DownloadSelected";
            public const string METHOD_RESOLVE_UPLOAD_WAITING = "ResolveUpload";
            public const string METHOD_APPLY_SORTING = "ApplySorting";
        }
    }
}
