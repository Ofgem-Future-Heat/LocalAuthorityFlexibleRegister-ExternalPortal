using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Change.Routes
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class Routes4Model(
        ILogger<Routes4Model> logger,
        Services.IDeclarationManagementService declarationManagementService)
        : PageModel
    {

        [BindProperty]
        public string? Urn { get; set; } = string.Empty;

        [BindProperty]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public string? ApplicationNumber { get; set; } = string.Empty;

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; }

        private Models.Declaration? Declaration { get; set; }


        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("Routes4Model - OnGetWithId");

            await RefreshData(declarationId, null);

            if (Declaration is null)
            {
                HasError = true;
                return;
            }

            DeclarationId = Declaration.DeclarationId;
            Urn = Declaration.Urn;
            ApplicationNumber = Declaration.Route4ApplicationNumber;
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("Routes4Model - OnPost");

            if (string.IsNullOrEmpty(ApplicationNumber))
            {
                DisplayMessage = ErrorMessagesExternalSite.Routes4.Route4ApplicationNumberIsAMandatoryField;
                HasError = true;
                return Page();
            }

            if (ApplicationNumber.Length != 5)
            {
                DisplayMessage = ErrorMessagesExternalSite.Routes4.Route4ApplicationNumberShouldBe5Digits;
                HasError = true;
                return Page();
            }

            await RefreshData(DeclarationId, ApplicationNumber);

            if (Declaration is null)
            {
                HasError = true;
                return Page();
            }

            Declaration.Route4ApplicationNumber = ApplicationNumber;
            Declaration.Route2Proxies = "";
            Declaration.AdditionalRoute2Proxies = "";

            declarationManagementService.UpdateDeclarationInCache(Declaration);

            return RedirectToPage(
                LafPages.DeclarationDetail.ROUTE,
                LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                new
                {
                    declarationId = DeclarationId
                });
        }



        private async Task RefreshData(Guid declarationId, string? preSelectedRoute)
        {
            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                ApplicationNumber = Declaration.ECO4orGreatBritishInsulationSchemeFlexReferralRoute;

                if (preSelectedRoute is not null)
                {
                    ApplicationNumber = preSelectedRoute;
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
