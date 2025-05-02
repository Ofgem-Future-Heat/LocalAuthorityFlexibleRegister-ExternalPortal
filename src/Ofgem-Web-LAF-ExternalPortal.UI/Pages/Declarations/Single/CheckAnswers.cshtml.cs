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
public class CheckAnswers(
    ILogger<CheckAnswers> logger,
    Services.IDeclarationManagementService declarationManagementService) : PageModel
{
    public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

    public string DisplayMessage { get; set; } = string.Empty;
    public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);
    public string? ErrorId { get; set; }

    public void OnGet()
    {
        logger.LogInformation("CheckAnswers - OnGet");

        try
        {
            SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                  ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

            TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CheckAnswers - OnGet ");
            DisplayMessage = "An issue occurred retrieving data";
        }
    }

    public async Task<IActionResult> OnPost()
    {
        logger.LogInformation("CheckAnswers - OnPost");

        try
        {
            SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                  throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

            var declaration = SingleDeclarationJourney.ToDeclaration();
            var saveResult = await declarationManagementService.SaveDeclarationAsync(declaration);
            var success = saveResult?.Responses is not null ;

            for (var i = 0; success && i < saveResult!.Responses!.Count; ++i)
            {
                //The assumption from the API is that response.Result should be an empty array, never null and never populated
                //Should we trust the response if response.Result is null?
                success = saveResult.Responses[i].Result != null && !saveResult.Responses[i].Result!.Any();
            }

            var rulesResult = await declarationManagementService.RunCoreRulesAsync(declaration);

            return RedirectToPage(LafPages.Confirmation.ROUTE, new
            {
                urn = rulesResult?.Urn,
                success = success
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CheckAnswers - OnPost ");
            DisplayMessage = "An issue occurred posting data";
        }

        return Page();
    }
}