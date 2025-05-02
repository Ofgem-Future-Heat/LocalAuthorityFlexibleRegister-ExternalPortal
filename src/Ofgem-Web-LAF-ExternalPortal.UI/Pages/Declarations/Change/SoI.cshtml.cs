using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Change
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class SoIModel(
        ILogger<SoIModel> logger,
        Services.IDeclarationManagementService declarationManagementService)
        : PageModel
    {

        [BindProperty]
        public string? Urn { get; set; } = string.Empty;

        [BindProperty]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public string? SoiLink { get; set; } = string.Empty;
        
        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; } 

        private Models.Declaration? Declaration { get; set; }


        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("SoIModel - OnGetWithId");

            await RefreshData(declarationId,null);

            if (Declaration is null)
            {
                HasError = true;
                return;
            }

            DeclarationId = Declaration.DeclarationId;
            Urn = Declaration.Urn;
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("SoIModel - OnPost");

            if (!UrlValidation.IsLinkValid(SoiLink))
            {
                HasError = true;
                DisplayMessage = ErrorMessagesExternalSite.SoI.SoILinkIsMandatory;
                return Page();
            }

            await RefreshData(DeclarationId,SoiLink);

            if (Declaration is null)
            {
                HasError = true;
                return Page();
            }

            Declaration.StatementOfIntentLink = SoiLink;

            declarationManagementService.UpdateDeclarationInCache(Declaration);

            return RedirectToPage(
                LafPages.DeclarationDetail.ROUTE,
                LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                new
                {
                    declarationId = DeclarationId
                });
        }

        private async Task RefreshData(Guid declarationId, string? preSelectedSoILink)
        {
            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                SoiLink = Declaration.StatementOfIntentLink;

                if (preSelectedSoILink is not null)
                {
                    SoiLink = preSelectedSoILink;
                }
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.GetDeclaration, $"An issue occurred retrieving declaration data, {ex.Message}");
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.AnIssueOccurredRetrievingData;
            }
        }
    }
}
