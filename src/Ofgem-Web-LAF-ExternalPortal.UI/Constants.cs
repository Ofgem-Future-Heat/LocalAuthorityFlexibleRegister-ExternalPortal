#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace Ofgem_Web_LAF_ExternalPortal;

public static class Constants
{
    public const string SOI_MINIMUM_DATE = "2000-01-01";

    public const int DECLARATIONS_PAGE_SIZE = 50;

    public const int SOI_PAGE_SIZE = 50;

    public const string LA_DURN_001_DECLARATION_UNIQUE_REFERENCE_NUMBER_IS_BLANK =
        "Column 'LA Declaration Unique Reference Number' is a required field. Please confirm the unique reference number, please refer to the Great British Insulation Scheme and ECO4 Flex Declaration Notification Data Dictionary for the correct format.";

    public const string LA_DURN_004_SINGLE_DECLARATION_UNIQUE_REFERENCE_NUMBER =
        "Please note the file cannot be uploaded because there are more than 1 submitting Local Authorities within it. Please note bulk upload is allowed for 1 Local Authority at a time.";

    public const string SOI_EDIT_MANDATORY_FIELDS_SIGN_OFF =
        "Mandatory fields on sign-off checklist are incomplete. Please review sign-off checklist and retry.";

    public const string VERSION_NUMBER_IS_INVALID_MESSAGE =
        "Please review mandatory field 'Statement of Intent version number' and retry. Only alphanumeric entries and the special character '.' and '-' and '_' are permitted.";

    public const string VERSION_NUMBER_IS_MANDATORY =
        "Mandatory fields are missing from this SoI. Please review mandatory fields and retry.";
    
    public const string INVALID_SOI_STATUS = "To change SoI Category to 'Complete', the SoI Status must be 'Valid'.";

    public const string Route1Text = "Route 1";
    public const string Route2Text = "Route 2";
    public const string Route3Text = "Route 3";
    public const string Route4Text = "Route 4 (ECO4 Flex only)";

    public const string TempDataReloadIssueText = "An issue occured while reloading the page";
    public const string UserClaimErrorText = "Unable to determine the users claims";


    public const string DeclarationCreationUploadText = "Upload";
    public const string DeclarationCreationSingleText = "Single";


    public const string NotApplicable = "N/A";
}