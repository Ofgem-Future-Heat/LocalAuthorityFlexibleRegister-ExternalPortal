using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Change
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class LaConsultedPriorToInstallationModel(
        ILogger<LaConsultedPriorToInstallationModel> logger,
        Services.IDeclarationManagementService declarationManagementService) : PageModel
    {
        [BindProperty] public string? Urn { get; set; } = string.Empty;

        [BindProperty] public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public string? Consulted { get; set; } = string.Empty;

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; }

        private Models.Declaration? Declaration { get; set; }

        public const string AnswerYes = "Yes";
        public const string AnswerNo = "No";

        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("LaConsultedPriorToInstallationModel - OnGetWithId");

            await RefreshData(declarationId, null);

            if (Declaration is null)
            {
                HasError = true;
                return;
            }

            DeclarationId = Declaration.DeclarationId;
            Urn = Declaration.Urn;
            Consulted = Declaration.LAWasConsultedPriorToInstallationCompletion;
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("LaConsultedPriorToInstallationModel - OnPost");

            if (string.IsNullOrEmpty(Consulted))
            {
                DisplayMessage = "Field is mandatory";
                HasError = true;
                return Page();
            }

            if (Consulted != AnswerYes && Consulted != AnswerNo)
            {
                DisplayMessage = "Field must be selected";
                HasError = true;
                return Page();
            }


            await RefreshData(DeclarationId, Consulted);

            if (Declaration is null)
            {
                HasError = true;
                return Page();
            }

            Declaration.LAWasConsultedPriorToInstallationCompletion = Consulted;

            declarationManagementService.UpdateDeclarationInCache(Declaration);

            return RedirectToPage(
                LafPages.DeclarationDetail.ROUTE,
                LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                new
                {
                    declarationId = DeclarationId
                });
        }

        private async Task RefreshData(Guid declarationId, string? preSelectedLaRemit)
        {
            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                Consulted = Declaration.ReferralMadeOutsideOfLAsRemit;

                if (preSelectedLaRemit is not null)
                {
                    Consulted = preSelectedLaRemit;
                }
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.GetDeclaration, $"An issue occurred retrieving declaration data, {ex.Message}");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }
    }
}