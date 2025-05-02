using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Single;
[AutoValidateAntiforgeryToken]
[Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
[Authorize(Policy = ClaimsConstants.LinkedAccount)]
[Authorize(Policy = "MustHaveClaimLocalAuthorities")]
public class SelectLocalAuthority(
    ILogger<SelectLocalAuthority> logger) : PageModel
{

    [BindProperty] public string? SelectedLa { get; set; } = string.Empty;

    public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

    public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);

    public string DisplayMessage { get; set; } = string.Empty;

    public string? ErrorId { get; set; }


    public void OnGet()
    {
        logger.LogInformation("SelectLocalAuthority - OnGet");

        try
        {
            SingleDeclarationJourney
            = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
            ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

            SelectedLa = SingleDeclarationJourney.OnsCode;

            TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SelectLocalAuthority - OnGet ");
            DisplayMessage = "An issue occurred retrieving data";
        }
    }


    public Task<IActionResult> OnPost()
    {
        logger.LogInformation("SelectLocalAuthority - OnPost");

        try
        {
            SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                  throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

            if (string.IsNullOrWhiteSpace(SelectedLa))
            {
                ErrorId = "SelectLa";
                DisplayMessage = "Select the local authority the declaration notification is being submitted for";
                return Task.FromResult<IActionResult>(Page());
            }

            SingleDeclarationJourney.OnsCode = SelectedLa;

            var source = SingleDeclarationJourney.LocalAuthorityList.Find(f => f.OnsCode == SelectedLa);

            if (source is null && SingleDeclarationJourney.BaseLocalAuthority?.OnsCode == SelectedLa)
            {
                source = SingleDeclarationJourney.BaseLocalAuthority;
            }

            SingleDeclarationJourney.OnsCodeDetails = $"{SelectedLa} {source?.Name}";


            TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

            return Task.FromResult<IActionResult>(RedirectToPage(LafPages.SelectStatementOfIntent.ROUTE));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SelectLocalAuthority - OnPost ");
            DisplayMessage = "An issue occurred posting data";
        }

        return Task.FromResult<IActionResult>(Page());
    }
}