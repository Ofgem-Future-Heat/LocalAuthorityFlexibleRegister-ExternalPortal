using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Models;
using Ofgem.LAF.SharedLibrary.Constants;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Single
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class UniqueReferenceNumber(
        ILogger<UniqueReferenceNumber> logger,
        Services.IDeclarationManagementService declarationManagementService) : PageModel
    {
        [BindProperty] public string? SelectedLa { get; set; } = string.Empty;

        [BindProperty] public string? SequentialNumber { get; set; } = string.Empty;

        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);

        public string DisplayMessage { get; set; } = string.Empty;

        public string? ErrorId { get; set; }

        public void OnGet()
        {
            logger.LogInformation("UniqueReferenceNumber - OnGet");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                      ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                SelectedLa = SingleDeclarationJourney.OnsCode;


                if (!string.IsNullOrWhiteSpace(SingleDeclarationJourney.UrnNumber))
                {
                    var splitUrn = SingleDeclarationJourney.UrnNumber.Split('-');
                    SequentialNumber = splitUrn[1];
                }
                else
                {
                    SequentialNumber = SingleDeclarationJourney.UrnNumber;
                }


                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UniqueReferenceNumber - OnGet ");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("UniqueReferenceNumber - OnPost");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                      throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                if (!ValidSequentialNumber())
                {
                    return Page();
                }

                var urnNumber = SelectedLa + "-" + SequentialNumber;

                // check URN unique

                var result = await declarationManagementService.GetDeclarationByUrn(urnNumber);

                // Scenario 2c: Triggered -  URN is duplicated
                if (result != null)
                {
                    ErrorId = "SequentialNumber";
                    DisplayMessage = RuleErrorMessages.DU_DECL_001_Urn_Is_Duplicated;
                    return Page();
                }

                SingleDeclarationJourney.UrnNumber = urnNumber;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return RedirectToPage(LafPages.LocalAuthorityHouseholder.ROUTE);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UniqueReferenceNumber - OnPost ");
                DisplayMessage = "An issue occurred posting data";
            }

            return Page();
        }

        internal bool ValidSequentialNumber()
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            // Scenario 2a: Triggered -  blank, Null, N/A

            if (string.IsNullOrEmpty(SequentialNumber))
            {
                ErrorId = "SequentialNumber";
                DisplayMessage = RuleErrorMessages.LA_DURN_001_Declaration_Unique_Reference_Number_Is_Blank;
                return false;
            }

            if (string.Equals(SequentialNumber, Constants.NotApplicable, StringComparison.OrdinalIgnoreCase))
            {
                ErrorId = "SequentialNumber";
                DisplayMessage = RuleErrorMessages.LA_DURN_001_Declaration_Unique_Reference_Number_Is_Blank;
                return false;
            }

            if (SequentialNumber.Contains(' '))
            {
                ErrorId = "SequentialNumber";
                DisplayMessage = RuleErrorMessages.LA_DURN_001_Declaration_Unique_Reference_Number_Is_Blank;
                return false;
            }


            // Scenario 2b: Triggered -  Incorrect format
            if (SequentialNumber.Length != 5)
            {
                ErrorId = "SequentialNumber";
                DisplayMessage = RuleErrorMessages.LA_DURN_002_Declaration_Unique_Reference_Number_Is_Not_In_The_Format_ANNNNNNNN_NNNNN;
                return false;
            }

            foreach (char numericPart in SequentialNumber)
            {
                if (!char.IsNumber(numericPart))
                {
                    ErrorId = "SequentialNumber";
                    DisplayMessage = RuleErrorMessages.LA_DURN_002_Declaration_Unique_Reference_Number_Is_Not_In_The_Format_ANNNNNNNN_NNNNN;
                    return false;
                }
            }

            return true;
        }
    }
}
