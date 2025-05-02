using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Single
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class SelectLocalAuthorityHouseholder(
        ILogger<SelectLocalAuthorityHouseholder> logger,
        Services.ILaManagementService laManagementService) : PageModel
    {
        [BindProperty] public string? SelectedLocalAuthorityHouseholder { get; set; } = string.Empty;

        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);

        public string DisplayMessage { get; set; } = string.Empty;

        public string? ErrorId { get; set; }

        public const string ErrorMessageId = "SelectLocalAuthorityHouseholder";

        public const string LA_ACODE_003_LA_Area_Code_Must_Exist_Error =
            "Ensure declaration notification area code is an existing area code";

        public void OnGet()
        {
            logger.LogInformation("SelectLocalAuthorityHouseholder - OnGet");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                      ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                SelectedLocalAuthorityHouseholder = SingleDeclarationJourney.OnsCode;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SelectLocalAuthorityHouseholder - OnGet ");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }


        public Task<IActionResult> OnPost()
        {
            logger.LogInformation("SelectLocalAuthorityHouseholder - OnPost");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                      throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");


                // scenario 1a: LA Area Code Must Be Provided

                if (string.IsNullOrWhiteSpace(SelectedLocalAuthorityHouseholder) || string.Equals(SelectedLocalAuthorityHouseholder, Constants.NotApplicable, StringComparison.OrdinalIgnoreCase)) 
                {
                    ErrorId = ErrorMessageId;
                    DisplayMessage = RuleErrorMessages.LA_ACODE_001_LA_Area_Code_Has_A_Value;
                    return Task.FromResult<IActionResult>(Page());
                }

                // scenario 1b: LA Area code is correct format
                if (!IsLocalAuthorityValid())
                {
                    return Task.FromResult<IActionResult>(Page());
                }

                // scenario 1c: LA Area Code Must Exists
                var result = laManagementService.GetByOnsCodeAsync(SelectedLocalAuthorityHouseholder);

                if (result.Result == null)
                {
                    ErrorId = ErrorMessageId;
                    DisplayMessage = LA_ACODE_003_LA_Area_Code_Must_Exist_Error;
                    return Task.FromResult<IActionResult>(Page());
                }

                SingleDeclarationJourney.HouseholdOnsCode = SelectedLocalAuthorityHouseholder;

                var source = SingleDeclarationJourney.LocalAuthorityList.Find(f => f.OnsCode == SelectedLocalAuthorityHouseholder);

                if (source is null && SingleDeclarationJourney.BaseLocalAuthority?.OnsCode == SelectedLocalAuthorityHouseholder)
                {
                    source = SingleDeclarationJourney.BaseLocalAuthority;
                }

                SingleDeclarationJourney.HouseholdOnsCodeDetails = $"{SelectedLocalAuthorityHouseholder} {source?.Name}";

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.SelectReferralRoute.ROUTE));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SelectLocalAuthorityHouseholder - OnPost ");
                DisplayMessage = "An issue occurred posting data";
            }

            return Task.FromResult<IActionResult>(Page());
        }

        internal bool IsLocalAuthorityValid()
        {
            if (SelectedLocalAuthorityHouseholder != null && SelectedLocalAuthorityHouseholder.Contains(' '))
            {
                ErrorId = ErrorMessageId;
                DisplayMessage = RuleErrorMessages.LA_ACODE_002_LA_Area_Code_Has_Correct_Format;
                return false;
            }

            // verify LA_Area_Code has 9 characters
            if (SelectedLocalAuthorityHouseholder?.Length != 9)
            {
                ErrorId = ErrorMessageId;
                DisplayMessage = RuleErrorMessages.LA_ACODE_002_LA_Area_Code_Has_Correct_Format;
                return false;
            }

            // verify LA_Area_Code first character is an alphabetic character
            char[] firstCharacter = SelectedLocalAuthorityHouseholder.ToCharArray(0, 9);
            if (!char.IsLetter(firstCharacter[0]))
            {
                ErrorId = ErrorMessageId;
                DisplayMessage = RuleErrorMessages.LA_ACODE_002_LA_Area_Code_Has_Correct_Format;
                return false;
            }

            // verify LA_Area_Code first character last 7 characters are numeric
            foreach (char numericPart in firstCharacter[1..])
            {
                if (!char.IsNumber(numericPart))
                {
                    ErrorId = ErrorMessageId;
                    DisplayMessage = RuleErrorMessages.LA_ACODE_002_LA_Area_Code_Has_Correct_Format;
                    return false;
                }
            }

            return true;
        }
    }
}
