#pragma warning disable CA1707
namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    /// <summary>
    /// Defines the structure of the pages and methods
    /// </summary>
    public static partial class LafPages
    {

        public static class SendDeclaration
        {
            public const string ROUTE = "/Declarations/SendDeclaration";
        }

        public static class UploadTemplate
        {
            public const string ROUTE = "/Declarations/Multiple/UploadTemplate";
        }

        public static class UploadTemplateConfirmation
        {
            public const string ROUTE = "/Declarations/Multiple/UploadIsOk";
        }
        public static class SelectLocalAuthority
        {
            public const string ROUTE = "/Declarations/Single/SelectLocalAuthority";
        }

        public static class SelectReferralRoute
        {
            public const string ROUTE = "/Declarations/Single/SelectReferralRoute";
        }

        public static class SelectStatementOfIntent
        {
            public const string ROUTE = "/Declarations/Single/SelectStatementOfIntent";
        }
        public static class UniqueReferenceNumber
        {
            public const string ROUTE = "/Declarations/Single/UniqueReferenceNumber";
        }

        public static class LocalAuthorityHouseholder
        {
            public const string ROUTE = "/Declarations/Single/SelectLocalAuthorityHouseholder";
        }

        public static class Proxies
        {
            public const string ROUTE = "/Declarations/Single/SelectRoute2Proxies";
        }

        public static class Route4ApplicationNumber
        {
            public const string ROUTE = "/Declarations/Single/Route4ApplicationNumber";
        }

        public static class AddressDetails
        {
            public const string ROUTE = "/Declarations/Single/AddressDetails";
        }

        public static class HouseholdEligibility
        {
            public const string ROUTE = "/Declarations/Single/HouseholdEligibility";
        }

        public static class WasLocalAuthorityConsulted
        {
            public const string ROUTE = "/Declarations/Single/WasLocalAuthorityConsulted";
        }

        public static class AdditionalProxies
        {
            public const string ROUTE = "/Declarations/Single/SelectAdditionalProxies";
        }

        public static class CheckAnswers
        {
            public const string ROUTE = "/Declarations/Single/CheckAnswers";
        }

        public static class Confirmation
        {
            public const string ROUTE = "/Declarations/Single/Confirmation";
        }

        public static class UploadReceived
        {
            public const string ROUTE = "/Declarations/Single/UploadReceived";
        }

        public static class DeclarationDetail
        {
            public const string ROUTE = "/Declarations/DeclarationDetail";
            public const string METHOD_USE_CACHED_DATA = "FromCache";
        }

        public static class DeclarationDetailEditCompleted
        {
            public const string ROUTE = "/Declarations/DeclarationDetailEditCompleted";
            public const string METHOD_GET_WITH_ID_AND_URN_ACTION = "WithIdAndUrn";
        }

        public static class AreaCode
        {
            public const string ROUTE = "/Declarations/Change/AreaCode";
        }

        public static class Routes
        {
            public const string ROUTE = "/Declarations/Change/Routes/Routes";
        }

        public static class Routes2
        {
            public const string ROUTE = "/Declarations/Change/Routes/Routes2";
        }

        public static class Routes4
        {
            public const string ROUTE = "/Declarations/Change/Routes/Routes4";
        }

        public static class Routes2Additional
        {
            public const string ROUTE = "/Declarations/Change/Routes/Routes2Additional";
        }

        public static class DateOfHouseHolderEligibility
        {
            public const string ROUTE = "/Declarations/Change/DateOfHouseHolderEligibility";
        }

        public static class DateOfStatementOfIntentPublication
        {
            public const string ROUTE = "/Declarations/Change/DateOfStatementOfIntentPublication";
        }

        public static class LaConsultedPriorToInstallation
        {
            public const string ROUTE = "/Declarations/Change/LaConsultedPriorToInstallation";
        }

        public static class StatementOfIntentLink
        {
            public const string ROUTE = "/Declarations/Change/SoI";
        }

        public static class StatementOfIntentPublishedFor
        {
            public const string ROUTE = "/Declarations/Change/StatementOfIntentPublishedFor";
        }

        public static class ReferralMadeOutsideOfLocalAuthorityRemit
        {
            public const string ROUTE = "/Declarations/Change/ReferralLaRemit";
        }

        public static class AddressOfHousehold
        {
            public const string ROUTE = "/Declarations/Change/AddressOfHousehold";
        }


    }
}
