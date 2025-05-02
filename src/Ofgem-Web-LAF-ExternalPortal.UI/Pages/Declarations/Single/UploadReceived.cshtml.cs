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
    public class UploadReceived(
        ILogger<UploadReceived> logger) : PageModel
    {
        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        public bool HasError { get; set; }

        public string DisplayMessage { get; set; } = string.Empty;

        public string? ErrorId { get; set; }

        public void OnGet()
        {
            logger.LogInformation("UploadReceived - OnGet");

            try
            {
                SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                  ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UploadReceived - OnGet");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public Task<IActionResult> OnPost()
        {
            logger.LogInformation("UploadReceived - OnPost");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                      throw new ArgumentNullException(nameof(SingleDeclarationJourney),
                          @"Single declaration data does not exist");

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.SendDeclaration.ROUTE));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UploadReceived - OnPost");
                DisplayMessage = "An issue occurred posting data";
            }

            return Task.FromResult<IActionResult>(Page());
        }
    }
}
