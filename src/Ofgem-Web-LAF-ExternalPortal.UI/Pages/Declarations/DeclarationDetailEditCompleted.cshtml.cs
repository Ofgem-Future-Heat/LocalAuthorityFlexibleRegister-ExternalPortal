using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]

    public class DeclarationDetailEditCompletedModel(ILogger<DeclarationDetailEditCompletedModel> logger) : PageModel
    {
        [BindProperty] public string DeclarationId { get; set; } = string.Empty;
        [BindProperty] public bool HasDeclarationId => !string.IsNullOrEmpty(DeclarationId);

        [BindProperty] public string Urn { get; set; } = string.Empty;

        public Task OnGetWithIdAndUrn(string declarationId, string urn)
        {
            logger.LogInformation("DeclarationDetailEditCompletedModel - OnGetWithId");

            DeclarationId = declarationId;
            Urn = urn;

            return Task.CompletedTask;
        }
    }
}
