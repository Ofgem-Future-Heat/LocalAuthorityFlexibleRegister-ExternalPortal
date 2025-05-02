#pragma warning disable CA1707
namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {

        public static class SOIDashboard
        {
            public const string ROUTE = "/Profiles/Soi/SoiDashboard";
            public const string METHOD_APPLY_FILTERS = "ApplyFilters";
            public const string METHOD_CLEAR_FILTERS = "ClearFilters";
            public const string METHOD_CLEAR_DATA = "ClearData";
        }

        public static class AddSoi
        {
            public const string ROUTE = "/Profiles/Soi/AddSoi";
            public const string METHOD_ADD = "Add";
            public const string METHOD_ADD_IN_LA = "AddInLa";
            public const string METHOD_TAKE_OUT_LA = "TakeOutLa";
            public const string METHOD_CAN_SUBMIT_ON_BEHALF_OF = "CanSubmitOnBehalfOf";
            public const string METHOD_CANNOT_SUBMIT_ON_BEHALF_OF = "CannotSubmitOnBehalfOf";

        }

        public static class AddSoiSummary
        {
            public const string ROUTE = "/Profiles/Soi/AddSoiSummary";
            public const string METHOD_ADD = "Add";
        }
        public static class AddSoiConfirmation
        {
            public const string ROUTE = "/Profiles/Soi/AddSoiConfirmation";
        }

        public static class SoiSummaryView
        {
            public const string ROUTE = "/Profiles/Soi/SoiSummaryView";
            public const string METHOD_GET_BY_ID = "ById";
        }

        public static class VersionNumberView
        {
            public const string ROUTE = "/Profiles/Soi/VersionNumberView";
            public const string METHOD_CONTINUE = "Continue";
        }

        public static class DatePublishedView
        {
            public const string ROUTE = "/Profiles/Soi/DatePublishedView";
            public const string METHOD_CONTINUE = "Continue";
        }

        public static class LinkView
        {
            public const string ROUTE = "/Profiles/Soi/LinkView";
            public const string METHOD_CONTINUE = "Continue";
        }
    }
}
