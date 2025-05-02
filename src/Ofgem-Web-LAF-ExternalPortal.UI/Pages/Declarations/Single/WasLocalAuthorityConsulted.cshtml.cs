using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public class WasLocalAuthorityConsulted(
        ILogger<WasLocalAuthorityConsulted> logger) : PageModel
    {
        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        [BindProperty] public bool? WasLocalAuthorityConsultedPriorToInstallation { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);

        public string DisplayMessage { get; set; } = string.Empty;

        public string? ErrorId { get; set; }

        public void OnGet()
        {
            logger.LogInformation("WasLocalAuthorityConsulted - OnGet");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                      ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                if (SingleDeclarationJourney.WasLocalAuthorityConsultedPriorToInstallation.HasValue)
                {
                    WasLocalAuthorityConsultedPriorToInstallation =
                        SingleDeclarationJourney.WasLocalAuthorityConsultedPriorToInstallation;
                }

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "WasLocalAuthorityConsulted - OnGet");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public Task<IActionResult> OnPost()
        {
            logger.LogInformation("WasLocalAuthorityConsulted - OnPost");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                      throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                if (!WasLocalAuthorityConsultedPriorToInstallation.HasValue)
                {
                    ErrorId = "WasLocalAuthorityConsultedPriorToInstallation";
                    DisplayMessage =
                        "Select an option to indicate whether the local authority has been consulted prior to installation";
                    return Task.FromResult<IActionResult>(Page());
                }

                SingleDeclarationJourney.WasLocalAuthorityConsultedPriorToInstallation
                    = WasLocalAuthorityConsultedPriorToInstallation.Value;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.CheckAnswers.ROUTE));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "WasLocalAuthorityConsulted - OnPost");
                DisplayMessage = "An issue occurred posting data";
            }

            return Task.FromResult<IActionResult>(Page());
        }
    }
}
