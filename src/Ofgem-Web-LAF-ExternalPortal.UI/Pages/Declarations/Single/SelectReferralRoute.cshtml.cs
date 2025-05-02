using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Enums;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Single;

[AutoValidateAntiforgeryToken]
[Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
[Authorize(Policy = ClaimsConstants.LinkedAccount)]
[Authorize(Policy = "MustHaveClaimLocalAuthorities")]
public class SelectReferralRoute(
    ILogger<SelectReferralRoute> logger) : PageModel
{
    [BindProperty] public ReferralRoutes SelectedRoute { get; set; } = ReferralRoutes.Unknown;

    public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

    public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);

    public string DisplayMessage { get; set; } = string.Empty;

    public string? ErrorId { get; set; }
    

    public void OnGet()
    {
        logger.LogInformation("SelectReferralRoute - OnGet");

        try
        {
            SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                  ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

            SelectedRoute = SingleDeclarationJourney.Route;

            TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SelectReferralRoute - OnGet");
            DisplayMessage = "An issue occurred retrieving data";
        }
    }


    public Task<IActionResult> OnPost()
    {
        logger.LogInformation("SelectReferralRoute - OnPost");

        try
        {
            SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                  throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

            if (!Enum.GetValues<ReferralRoutes>().Contains(SelectedRoute) || SelectedRoute == ReferralRoutes.Unknown)
            {
                ErrorId = "SelectRoute";
                DisplayMessage = "Select the referral route";
                return Task.FromResult<IActionResult>(Page());
            }

            SingleDeclarationJourney.Route = SelectedRoute;

            switch (SelectedRoute)
            {
                case ReferralRoutes.Unknown:
                    break;
                case ReferralRoutes.HouseHoldIncome:
                    SingleDeclarationJourney.RouteDetail = Routes.Route1;
                    break;
                case ReferralRoutes.ProxyTargeting:
                    SingleDeclarationJourney.RouteDetail = Routes.Route2;
                    break;
                case ReferralRoutes.NhsReferrals:
                    SingleDeclarationJourney.RouteDetail = Routes.Route3;
                    break;
                case ReferralRoutes.BespokeTargeting:
                    SingleDeclarationJourney.RouteDetail = Routes.Route4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

            return SelectedRoute switch
            {
                ReferralRoutes.HouseHoldIncome => Task.FromResult<IActionResult>(
                    RedirectToPage(LafPages.AddressDetails.ROUTE)),
                ReferralRoutes.ProxyTargeting => Task.FromResult<IActionResult>(RedirectToPage(LafPages.Proxies.ROUTE)),
                ReferralRoutes.NhsReferrals => Task.FromResult<IActionResult>(
                    RedirectToPage(LafPages.AddressDetails.ROUTE)),
                ReferralRoutes.BespokeTargeting => Task.FromResult<IActionResult>(
                    RedirectToPage(LafPages.Route4ApplicationNumber.ROUTE)),
                ReferralRoutes.Unknown => throw new ArgumentException(@"Invalid Referral Route", nameof(SelectedRoute)),
                _ => throw new ArgumentException(@"Invalid Referral Route", nameof(SelectedRoute))
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SelectReferralRoute - OnPost");
            DisplayMessage = "An issue occurred posting data";
        }

        return Task.FromResult<IActionResult>(Page());
    }
}