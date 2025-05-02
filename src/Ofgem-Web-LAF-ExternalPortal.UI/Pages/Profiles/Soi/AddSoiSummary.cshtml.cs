using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Services;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Profiles.Soi
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class AddSoiSummary
        (
            ILogger<AddSoiSummary> logger,
            ILaManagementService laManagementService) : SoiPage 
    {
        [BindProperty] public string OnsCode { get; set; } = string.Empty;

        // page values
        [BindProperty] public string SoiVersion { get; set; } = string.Empty;
        [BindProperty] public Models.ThreePartDate? DatePublishedControl { get; set; }
        [BindProperty] public DateTime DatePublished { get; set; }
        [BindProperty] public string SoiLink { get; set; } = string.Empty;


        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;
        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;


        public void OnGet()
        {
            var request = InitialisePage();

            if (request is null)
            {
                DisplayMessage = "An issue occured while reloading the page";
                logger.LogError("An issue occured while reloading the page");
            }
        }

        public async Task<IActionResult> OnPostAdd()
        {
            var request = InitialisePage();

            if (request is null)
            {
                DisplayMessage = "An issue occured while reloading the page";
                logger.LogError("An issue occured while reloading the page");
                return Page();
            }

            var (soiResult, success, errorMessage) = await laManagementService.CreateSoiAsync(request);

            if (success)
            {
                return RedirectToPage(LafPages.AddSoiConfirmation.ROUTE, new
                {
                    statementOfIntentId = soiResult?.StatementOfIntentId
                });
            }

            if (errorMessage != null) DisplayMessage = errorMessage;

            return Page();
        }

        private Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest? InitialisePage()
        {
            var request = TempData.Get<Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest>(TempDataKeys.StatementOfIntentData);

            if (request is null)
            {
                return null;
            }

            if (request.OnsCode is not null) OnsCode = request.OnsCode;
            if (request.VersionNumber != null) SoiVersion = request.VersionNumber;
            DatePublished = request.PublishedDate;
            if (request.StatementOfIntentLink != null) SoiLink = request.StatementOfIntentLink;

            TempData.Put(TempDataKeys.StatementOfIntentData, request);

            return request;
        }
    }
}