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
    public class SelectAdditionalProxies(
        ILogger<SelectAdditionalProxies> logger) : PageModel
    {
        [BindProperty] public string? SelectedAdditionalProxy { get; set; } = string.Empty;

        [BindProperty] public string DoNotShowRoute { get; set; } = string.Empty;

        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        public bool HasError { get; set; }

        public string DisplayMessage { get; set; } = string.Empty;

        public string? ErrorId { get; set; }

        public void OnGet()
        {
            logger.LogInformation("SelectAdditionalProxies - OnGet");

            try
            {
                SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                  ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");
                
                DoNotShowRoute = SingleDeclarationJourney.Route2ProxyHouseholdReferred;

                SelectedAdditionalProxy = SingleDeclarationJourney.Route2ProxyHouseholdReferredAdditional;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SelectAdditionalProxies - OnGet");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public Task<IActionResult> OnPost()
        {
            logger.LogInformation("SelectAdditionalProxies - OnPost");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                      throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                if (string.IsNullOrEmpty(SelectedAdditionalProxy))
                {
                    HasError = true;
                    ErrorId = "SelectRouteProxy";
                    DisplayMessage = "Select an additional proxy";
                    return Task.FromResult<IActionResult>(Page());
                }

                var valid = SelectedAdditionalProxy is 
                    Route2Proxy.Route2Proxy1Text or 
                    Route2Proxy.Route2Proxy2Text or 
                    Route2Proxy.Route2Proxy3Text or 
                    Route2Proxy.Route2Proxy4Text or 
                    Route2Proxy.Route2Proxy5Text or 
                    Route2Proxy.Route2Proxy6Text or 
                    Route2Proxy.Route2Proxy7PPMText or 
                    Route2Proxy.Route2Proxy7NonPPMText;

                if (!valid)
                {
                    DisplayMessage = "Unknown value supplied for Route 2 additional proxy";
                    HasError = true;
                    return Task.FromResult<IActionResult>(Page());
                }

                SingleDeclarationJourney.Route2ProxyHouseholdReferredAdditional = SelectedAdditionalProxy;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.AddressDetails.ROUTE));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SelectAdditionalProxies - OnGet");
                DisplayMessage = "An issue occurred posting data";
            }

            return Task.FromResult<IActionResult>(Page());
        }
    }
}
