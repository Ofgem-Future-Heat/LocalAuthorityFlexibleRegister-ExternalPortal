using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Change
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class DateOfHouseHolderEligibilityModel(ILogger<DateOfHouseHolderEligibilityModel> logger,
        Services.IDeclarationManagementService declarationManagementService) : PageModel
    {
        [BindProperty]
        public string? Urn { get; set; } = string.Empty;

        [BindProperty]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public Models.ThreePartDate DateOfHouseholderEligibilityControl { get; set; } = new();

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; }

        private Models.Declaration? Declaration { get; set; }

        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("DateOfHouseHolderEligibilityModel - OnGetWithId");

            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                DeclarationId = Declaration.DeclarationId;
                Urn = Declaration.Urn;

                DateOfHouseholderEligibilityControl
                    = new Models.ThreePartDate(
                        source: DateTime.Parse(Declaration.DateOfHouseholderEligibility ?? DateTime.Today.ToShortDateString(), new CultureInfo("en-GB")),
                        title: "What date did the household become eligible for the scheme?",
                        titleToBeUsedInErrorMessage: "Date of Householder Eligibility",
                        firstHint: "",
                        secondHint: "For example, 25 2 2024",
                        showTheHighlightBar: true
                    );

            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.GetDeclaration, $"An issue occurred retrieving declaration data, {ex.Message}");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("DateOfHouseHolderEligibilityModel - OnPost");

            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(DeclarationId)
                              ?? throw new ArgumentNullException($"{DeclarationId} does not exist");


                if (DateOfHouseholderEligibilityControl.HasErrors())
                {
                    HasError = true;
                    DisplayMessage = DateOfHouseholderEligibilityControl.ErrorMessage;
                    return Page();
                }

                var dateOfHouseholderEligibility =
                    new DateTime(
                        DateOfHouseholderEligibilityControl.Year,
                        DateOfHouseholderEligibilityControl.Month,
                        DateOfHouseholderEligibilityControl.Day, 0, 0, 0, DateTimeKind.Utc);

                Declaration.DateOfHouseholderEligibility =
                    dateOfHouseholderEligibility.ToString("dd/MM/yyyy");

                declarationManagementService.UpdateDeclarationInCache(Declaration);

                return RedirectToPage(
                    LafPages.DeclarationDetail.ROUTE,
                    LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                    new
                    {
                        declarationId = DeclarationId
                    });

            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.GetDeclaration,
                    $"An issue occurred retrieving declaration data, {ex.Message}");
                DisplayMessage = "An issue occurred retrieving data";
            }

            return Page();
        }
    }
}
