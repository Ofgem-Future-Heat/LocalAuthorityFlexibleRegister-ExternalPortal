using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]

    public class DeclarationDetailModel(
        ILogger<DeclarationDetailModel> logger,
        Services.IDeclarationManagementService declarationManagementService,
        Services.ILaManagementService laManagementService,
        IHttpContextAccessor httpContextAccessor)
        : PageModel
    {
        [BindProperty]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty]
        public Models.Declaration? Declaration { get; set; }

        [BindProperty]
        public Models.DeclarationErrorFields DeclarationErrorFields { get; set; } = new();

        [BindProperty]
        public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty]
        public bool HasChanges { get; set; }

        public bool SafeToShow { get; set; }


        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("DeclarationDetailModel - OnGetWithId");

            DeclarationId = declarationId;

            declarationManagementService.RemoveDeclarationFromCache(declarationId);

            await RefreshData(declarationId);
        }

        public async Task OnGetFromCache(Guid declarationId)
        {
            logger.LogInformation("DeclarationDetailModel - OnGetFromCache");

            DeclarationId = declarationId;

            await RefreshDataFromCacheAsync(declarationId);

            HasChanges = true;
        }

        public async Task<IActionResult> OnPost(Guid declarationId)
        {
            logger.LogInformation("DeclarationDetailModel - OnPost");

            try
            {
                DeclarationId = declarationId;

                await RefreshDataFromCacheAsync(declarationId);

                if (Declaration is not null)
                {
                    var result = declarationManagementService.SaveDeclarationAsync(Declaration);

                    if (result.Result != null)
                    {
                        return RedirectToPage(
                            LafPages.DeclarationDetailEditCompleted.ROUTE,
                            LafPages.DeclarationDetailEditCompleted.METHOD_GET_WITH_ID_AND_URN_ACTION,
                            new
                            {
                                declarationId = result.Result.NewDeclarationId,
                                urn = Declaration.Urn
                            });
                    }

                    logger.LogError(null, "DeclarationDetailModel - OnPost - Failed: ");
                    DisplayMessage = ErrorMessagesExternalSite.DeclarationDetail.AnIssueOccurredUpdatingTheRecord;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "DeclarationDetailModel - OnPost - Failed Declaration ID: {DeclarationId}",
                    declarationId);
                DisplayMessage = ErrorMessagesExternalSite.DeclarationDetail.AnIssueOccurredUpdatingTheRecord;
            }

            return Page();
        }

        private async Task RefreshData(Guid declarationId)
        {
            try
            {
                if (httpContextAccessor.HttpContext is null)
                {
                    DisplayMessage = "Error occured loading the page";
                    logger.LogLafError(LogEvents.GetDeclaration, DisplayMessage);
                    throw new ArgumentNullException("DeclarationDetailModel - httpContextAccessor.HttpContext is null");
                }


                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                DeclarationErrorFields = new Models.DeclarationErrorFields(Declaration.DeclarationErrors);

                // Set SafeToShow based on user claims
                SetSafeToShow();

                // if status is 0 then get soi status with api
                if (Declaration.SoiStatus == 0)
                {
                    if (string.IsNullOrWhiteSpace(Declaration.LAAreaCode) ||
                        string.IsNullOrWhiteSpace(Declaration.DateOfStatementOfIntentPublication))
                    {
                        return;
                    }

                    var soiResult = await laManagementService.GetSoiByOns(Declaration.LAAreaCode, Declaration.DateOfStatementOfIntentPublication);

                    if (soiResult == null)
                    {
                        return;
                    }

                    Declaration.SoiStatus = soiResult.Status;
                }
            }
            catch (Exception ex)
            {
                DisplayMessage = $"An issue occurred retrieving declaration data, {ex.Message}";
                logger.LogLafError(LogEvents.GetDeclaration, DisplayMessage);
            }
        }

        private async Task RefreshDataFromCacheAsync(Guid declarationId)
        {
            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                var ruleEngineResult = await declarationManagementService.RunCoreRulesAsync(Declaration);

                DeclarationErrorFields = new Models.DeclarationErrorFields(ruleEngineResult.DeclarationErrors);

                // Set SafeToShow based on user claims
                SetSafeToShow();
            }
            catch (Exception ex)
            {
                DisplayMessage = $"An issue occurred retrieving declaration data, {ex.Message}";
                logger.LogLafError(LogEvents.GetDeclaration, DisplayMessage);
            }
        }

        private void SetSafeToShow()
        {
            if (httpContextAccessor.HttpContext != null)
            {
                var claims = new Models.Claims(httpContextAccessor.HttpContext.User);

                var acceptableLaCodes = string.Join(", ", claims.AsArrayOfStrings());

                SafeToShow = acceptableLaCodes.Contains(Declaration?.LAAreaCode ?? string.Empty);
            }
        }
    }
}
