#pragma warning disable CA1707
namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {

        public static class ExternalUsers
        {
            public const string ROUTE = "/ExternalUsers/ExternalUsers";
            public const string METHOD_APPLY_FILTERS = "ApplyFilters";
            public const string METHOD_CLEAR_FILTERS = "ClearFilters";

        }

        public static class ExternalUserAdd
        {
            public const string ROUTE = "/ExternalUsers/AddExternalUser";
            public const string METHOD_SAVE = "Save";

        }

        public static class ExternalUserEdit
        {
            public const string ROUTE = "/ExternalUsers/EditExternalUser";
            public const string METHOD_GET_BY_ID = "ById";
            public const string METHOD_UPDATE = "Update";
        }

        public static class ExternalUserCompleted
        {
            public const string ROUTE = "/ExternalUsers/ExternalUserCompleted";

            public const string METHOD_GET_SUCCESS_ADD = "SuccessAdd";

            public const string METHOD_GET_SUCCESS_EDIT = "SuccessEdit";

            public const string METHOD_GET_SUCCESS_REACTIVATE = "SuccessReactivate";

            public const string METHOD_GET_SUCCESS_DEACTIVATION = "SuccessDeactivation";
        }

        public static class ExternalUserDelete
        {
            public const string ROUTE = "/ExternalUsers/DeactivateExternalUser";
            public const string METHOD_GET_BY_ID = "ById";
            public const string METHOD_DEACTIVATE = "deactivate";
        }

        public static class ExternalUserReactivate
        {
            public const string ROUTE = "/ExternalUsers/ReactivateExternalUser";
            public const string METHOD_GET_BY_ID = "ById";
            public const string METHOD_REACTIVATE = "Reactivate";
        }
        public static class ExternalUserView
        {
            public const string ROUTE = "/ExternalUsers/ViewExternalUser";

        }
    }
}
