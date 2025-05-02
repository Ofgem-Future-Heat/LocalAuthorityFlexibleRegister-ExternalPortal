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
    public class Route4ApplicationNumber(
        ILogger<Route4ApplicationNumber> logger) : PageModel
    {
        [BindProperty] public string? ApplicationNumber { get; set; } = string.Empty;

        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);

        public string DisplayMessage { get; set; } = string.Empty;

        public string? ErrorId { get; set; }

        public void OnGet()
        {
            logger.LogInformation("Route4ApplicationNumber - OnGet");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                      ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                ApplicationNumber = SingleDeclarationJourney.Route4ApplicationNumber;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Route4ApplicationNumber - OnGet");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public Task<IActionResult> OnPost()
        {
            logger.LogInformation("Route4ApplicationNumber - OnPost");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                      throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");
                if (string.IsNullOrEmpty(ApplicationNumber))
                {
                    DisplayMessage = "Enter the route 4 application number";
                    ErrorId = "ApplicationNumber";
                    return Task.FromResult<IActionResult>(Page());
                }

                if (ApplicationNumber.Length != 5)
                {
                    DisplayMessage = RuleErrorMessages.RE_ROUTE4_001_Route_4_Application_Has_Incorrect_Format;
                    ErrorId = "ApplicationNumber";
                    return Task.FromResult<IActionResult>(Page());
                }

                SingleDeclarationJourney.Route4ApplicationNumber = ApplicationNumber;


                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.AddressDetails.ROUTE));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Route4ApplicationNumber - OnPost");
                DisplayMessage = "An issue occurred posting data";
            }

            return Task.FromResult<IActionResult>(Page());
        }
    }
}
