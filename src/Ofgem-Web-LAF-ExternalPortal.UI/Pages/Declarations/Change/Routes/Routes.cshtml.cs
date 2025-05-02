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
    public class RoutesModel(
        ILogger<RoutesModel> logger,
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
            logger.LogInformation("RoutesModel - OnGetWithId");

            await RefreshData(declarationId, null);

            if (Declaration is null)
            {
                HasError = true;
                return;
            }

            DeclarationId = Declaration.DeclarationId;
            Urn = Declaration.Urn;
            SelectedRoute = Declaration.ECO4orGreatBritishInsulationSchemeFlexReferralRoute;
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("RoutesModel - OnPost");

            if (string.IsNullOrEmpty(SelectedRoute))
            {
                DisplayMessage = ErrorMessagesExternalSite.Routes.RouteIsAMandatoryField;
                HasError = true;
                return Page();
            }

            await RefreshData(DeclarationId, SelectedRoute);

            if (Declaration is null)
            {
                HasError = true;
                return Page();
            }

            Declaration.ECO4orGreatBritishInsulationSchemeFlexReferralRoute = SelectedRoute;

            declarationManagementService.UpdateDeclarationInCache(Declaration);

            switch (SelectedRoute)
            {
                case Constants.Route1Text:

                    Declaration.Route2Proxies = "";
                    Declaration.AdditionalRoute2Proxies = "";
                    Declaration.Route4ApplicationNumber= "";
                    
                    return RedirectToPage(
                        LafPages.DeclarationDetail.ROUTE,
                        LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                        new
                        {
                            declarationId = DeclarationId
                        });

                case Constants.Route2Text:

                    Declaration.Route4ApplicationNumber = "";

                    return RedirectToPage(
                        LafPages.Routes2.ROUTE,
                        LafPages.METHOD_GET_WITH_ID_ACTION,
                        new
                        {
                            declarationId = DeclarationId
                        });

                case Constants.Route3Text:

                    Declaration.Route2Proxies = "";
                    Declaration.AdditionalRoute2Proxies = "";
                    Declaration.Route4ApplicationNumber = "";

                    declarationManagementService.UpdateDeclarationInCache(Declaration);

                    return RedirectToPage(
                        LafPages.DeclarationDetail.ROUTE,
                        LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                        new
                        {
                            declarationId = DeclarationId
                        });

                case Constants.Route4Text:

                    Declaration.Route2Proxies = "";
                    Declaration.AdditionalRoute2Proxies = "";
                    Declaration.Route4ApplicationNumber = "";

                    return RedirectToPage(
                        LafPages.Routes4.ROUTE,
                        LafPages.METHOD_GET_WITH_ID_ACTION,
                        new
                        {
                            declarationId = DeclarationId
                        });

                default:
                    DisplayMessage = ErrorMessagesExternalSite.Routes.RouteMustBeSelected;
                    HasError = true;
                    break;
            }

            return Page();
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
