using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Single;

[AutoValidateAntiforgeryToken]
[Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
[Authorize(Policy = ClaimsConstants.LinkedAccount)]
[Authorize(Policy = "MustHaveClaimLocalAuthorities")]
public class Confirmation(
    ILogger<Confirmation> logger) : PageModel
{
    public string UrnNumber { get; set; } = string.Empty;

    public bool HasError { get; set; }

    public void OnGet(string urn, bool success)
    {
        logger.LogInformation("Confirmation - OnGet");

        UrnNumber = urn;
        HasError = !success;

        TempData.Remove(TempDataKeys.SingleDeclarationData);
    }
}