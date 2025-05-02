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
    public class SoiPublishedForModel(
        ILogger<SoiPublishedForModel> logger,
        Services.IDeclarationManagementService declarationManagementService) : PageModel
    {
        [BindProperty] public string? Urn { get; set; } = string.Empty;

        [BindProperty] public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public string? SoiPublishedFor { get; set; } = string.Empty;

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; }

        private Models.Declaration? Declaration { get; set; }

        public const string SoiPublishedForECO4 = ErrorMessagesExternalSite.SoiPublishedFor.Eco4Flex;
        public const string SoiPublishedForGBIS = ErrorMessagesExternalSite.SoiPublishedFor.GBIS;
        public const string SoiPublishedForECO4AndGBIS = "ECO4 Flex and GBIS";

        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("SoiPublishedForModel - OnGetWithId");

            await RefreshData(declarationId, null);

            if (Declaration is null)
            {
                HasError = true;
                return;
            }

            DeclarationId = Declaration.DeclarationId;
            Urn = Declaration.Urn;
            SoiPublishedFor = Declaration.StatementOfIntentPublishedFor;
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("SoiPublishedForModel - OnPost");

            if (string.IsNullOrEmpty(SoiPublishedFor))
            {
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.FieldIsMandatory;
                HasError = true;
                return Page();
            }

            if (SoiPublishedFor != SoiPublishedForECO4 && SoiPublishedFor != SoiPublishedForGBIS &&
                SoiPublishedFor != SoiPublishedForECO4AndGBIS)
            {
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.FieldMustBeSelected;
                HasError = true;
                return Page();
            }


            await RefreshData(DeclarationId, SoiPublishedFor);

            if (Declaration is null)
            {
                HasError = true;
                return Page();
            }

            Declaration.StatementOfIntentPublishedFor = SoiPublishedFor;

            declarationManagementService.UpdateDeclarationInCache(Declaration);

            return RedirectToPage(
                LafPages.DeclarationDetail.ROUTE,
                LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                new
                {
                    declarationId = DeclarationId
                });
        }

        private async Task RefreshData(Guid declarationId, string? preSelectedPublishedFor)
        {
            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                SoiPublishedFor = Declaration.StatementOfIntentPublishedFor;

                if (preSelectedPublishedFor is not null)
                {
                    SoiPublishedFor = preSelectedPublishedFor;
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