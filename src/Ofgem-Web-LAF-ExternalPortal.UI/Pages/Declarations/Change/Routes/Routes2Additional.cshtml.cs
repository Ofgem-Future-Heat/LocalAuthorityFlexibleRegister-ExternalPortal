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
    public class Routes2AdditionalModel(
        ILogger<Routes2AdditionalModel> logger,
        Services.IDeclarationManagementService declarationManagementService)
        : PageModel
    {

        [BindProperty]
        public string? Urn { get; set; } = string.Empty;

        [BindProperty]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public string? SelectedRoute { get; set; } = string.Empty;

        [BindProperty] public string? DoNotShowRoute { get; set; } = string.Empty;

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; }

        private Models.Declaration? Declaration { get; set; }

        public async Task OnGetWithId(Guid declarationId, string doNotShowRoute)
        {
            logger.LogInformation("Routes2AdditionalModel - OnGetWithId");

            await RefreshData(declarationId, null);

            if (Declaration is null)
            {
                HasError = true;
                return;
            }

            DeclarationId = Declaration.DeclarationId;
            Urn = Declaration.Urn;
            SelectedRoute = Declaration.AdditionalRoute2Proxies;
            DoNotShowRoute = doNotShowRoute;
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("Routes2AdditionalModel - OnPost");

            if (string.IsNullOrEmpty(SelectedRoute))
            {
                DisplayMessage = ErrorMessagesExternalSite.Routes2Additional.Route2AdditionalProxyIsAMandatoryField;
                HasError = true;
                return Page();
            }

            var valid = SelectedRoute == Extensions.Route2Proxy.Route2Proxy1Text ||
                        SelectedRoute == Extensions.Route2Proxy.Route2Proxy2Text ||
                        SelectedRoute == Extensions.Route2Proxy.Route2Proxy3Text ||
                        SelectedRoute == Extensions.Route2Proxy.Route2Proxy4Text ||
                        SelectedRoute == Extensions.Route2Proxy.Route2Proxy5Text ||
                        SelectedRoute == Extensions.Route2Proxy.Route2Proxy6Text ||
                        SelectedRoute == Extensions.Route2Proxy.Route2Proxy7PPMText ||
                        SelectedRoute == Extensions.Route2Proxy.Route2Proxy7NonPPMText;

            if (!valid)
            {
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.UnknownValueSuppliedForRoute2AdditionalProxy;
                HasError = true;
                return Page();
            }

            await RefreshData(DeclarationId, SelectedRoute);

            if (Declaration is null)
            {
                HasError = true;
                return Page();
            }

            Declaration.AdditionalRoute2Proxies = SelectedRoute;

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

                SelectedRoute = Declaration.ECO4orGreatBritishInsulationSchemeFlexReferralRoute;

                if (preSelectedRoute is not null)
                {
                    SelectedRoute = preSelectedRoute;
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
