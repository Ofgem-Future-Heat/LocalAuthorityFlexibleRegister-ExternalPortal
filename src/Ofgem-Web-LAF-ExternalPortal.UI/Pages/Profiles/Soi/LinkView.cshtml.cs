using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Profiles.Soi
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class LinkViewModel(
        ILogger<LinkViewModel> logger,
        IHttpContextAccessor httpContextAccessor,
        Services.ILaManagementService laManagementService) : SoiPage
    {
        private Services.ILaManagementService LaManagementService { get; set; } = laManagementService;

        private const string PageName = "LinkViewModel";

        public Models.Claims? Claims { get; set; } = new(httpContextAccessor.HttpContext ?? throw new InvalidOperationException());

        // claim information
        [BindProperty] public string OnsCode { get; set; } = string.Empty;
        [BindProperty] public string BaseLocalAuthorityName { get; set; } = string.Empty;
        [BindProperty] public int BaseLocalAuthorityId { get; set; }

        // page values
        [BindProperty] public string SoiLink { get; set; } = string.Empty;
        [BindProperty] public string SoiVersion { get; set; } = string.Empty;
        [BindProperty] public Models.ThreePartDate? DatePublishedControl { get; set; }
        [BindProperty] public DateTime DatePublished { get; set; }

        // error flags
        [BindProperty] public bool SoiLinkHasError { get; set; }
        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;
        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;


        public void OnGet()
        {
            logger.LogInformation("{PageName} -  OnGet", PageName);

            var request = InitialisePage();

            if (request is null)
            {
                DisplayMessage = Constants.TempDataReloadIssueText;
                logger.LogError(Constants.TempDataReloadIssueText);
            }
        }

        public async Task<IActionResult> OnPostContinue()
        {
            logger.LogInformation("{PageName} -  OnPostContinue", PageName);

            var initData = InitialisePage();

            if (initData is null)
            {
                DisplayMessage = Constants.TempDataReloadIssueText;
                logger.LogError(Constants.TempDataReloadIssueText);
            }

            if (!ValidBasePage()) return Page();

            var baseLa = await LaManagementService.GetByOnsCodeAsync(OnsCode);

            if (baseLa is null)
            {
                DisplayMessage = "Unable to load the base local authority";
                return Page();
            }

            Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest request = new()
            {
                OnsCode = OnsCode,
                LocalAuthorityId = baseLa.LocalAuthorityId,
                PublishedDate = DatePublished,
                Status = SoiStatusV2.ToBeAssessed,
                VersionNumber = SoiVersion,
                Category = SoiCategory.Incomplete,
                CanSubmit = false,
                StatementOfIntentLink = SoiLink,
                DesignatedLas = []
            };

            TempData.Put(TempDataKeys.StatementOfIntentData, request);

            return RedirectToPage(LafPages.AddSoiSummary.ROUTE);
        }

        private bool ValidBasePage()
        {
            if (!UrlValidation.IsLinkValid(SoiLink))
            {
                SoiLinkHasError = true;
                DisplayMessage = "SoI link is mandatory and must be a hyperlink.";
                return false;
            }

            return true;
        }

        private Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest? InitialisePage()
        {
            // get the prior data values
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

