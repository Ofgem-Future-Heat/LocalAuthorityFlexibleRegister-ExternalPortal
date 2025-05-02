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
    public class SelectRoute2Proxies(
        ILogger<SelectRoute2Proxies> logger) : PageModel
    {
        [BindProperty] public string? SelectedRoute2Proxy { get; set; } = string.Empty;

        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);

        public string DisplayMessage { get; set; } = string.Empty;

        public string? ErrorId { get; set; }


        public void OnGet()
        {
            logger.LogInformation("SelectRoute2Proxies - OnGet");

            try
            {
                SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                  ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                SelectedRoute2Proxy = SingleDeclarationJourney.Route2ProxyHouseholdReferred;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SelectRoute2Proxies - OnGet");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public Task<IActionResult> OnPost()
        {
            logger.LogInformation("SelectRoute2Proxies - OnPost");

            try
            {
                SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                  throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                if (string.IsNullOrEmpty(SelectedRoute2Proxy))
                {
                    ErrorId = "SelectRouteProxy";
                    DisplayMessage = "Select the Route 2 Proxy";
                    return Task.FromResult<IActionResult>(Page());
                }

                var valid = SelectedRoute2Proxy is 
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
                    DisplayMessage = "Unknown value supplied for Route 2 proxy";
                    ErrorId = "SelectRouteProxy";
                    return Task.FromResult<IActionResult>(Page());
                }

                SingleDeclarationJourney.Route2ProxyHouseholdReferred = SelectedRoute2Proxy;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.AdditionalProxies.ROUTE));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SelectRoute2Proxies - OnPost");
                DisplayMessage = "An issue occurred posting data";
            }

            return Task.FromResult<IActionResult>(Page());
        }
    }
}
