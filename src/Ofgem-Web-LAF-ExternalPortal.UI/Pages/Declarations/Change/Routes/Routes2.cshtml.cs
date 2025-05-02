using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Change.Routes
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class Routes2Model(
        ILogger<Routes2Model> logger,
        Services.IDeclarationManagementService declarationManagementService)
        : PageModel
    {
        [BindProperty]
        public string? Urn { get; set; } = string.Empty;

        [BindProperty]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public string? SelectedRoute { get; set; } = string.Empty;

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; }

        private Models.Declaration? Declaration { get; set; }

        

        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("Routes2Model - OnGetWithId");

            await RefreshData(declarationId, null);

            if (Declaration is null)
            {
                HasError = true;
                return;
            }

            DeclarationId = Declaration.DeclarationId;
            Urn = Declaration.Urn;
            SelectedRoute = Declaration.Route2Proxies;
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("Routes2Model - OnPost");

            if (string.IsNullOrEmpty(SelectedRoute))
            {
                DisplayMessage = ErrorMessagesExternalSite.Routes2.Route2IsAMandatoryField;
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

            Declaration.Route2Proxies = SelectedRoute;

            declarationManagementService.UpdateDeclarationInCache(Declaration);



            return RedirectToPage(
                Extensions.LafPages.Routes2Additional.ROUTE,
                Extensions.LafPages.METHOD_GET_WITH_ID_ACTION,
                new
                {
                    declarationId = Declaration.DeclarationId,
                    doNotShowRoute = Declaration.Route2Proxies 
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
