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
    public class VersionNumberViewModel(
        ILogger<VersionNumberViewModel> logger,
        IHttpContextAccessor httpContextAccessor,
        Services.ILaManagementService laManagementService) : SoiPage
    {
        private Services.ILaManagementService LaManagementService { get; set; } = laManagementService;

        private const string PageName = "VersionNumberView";

        public Models.Claims? Claims { get; set; } = new(httpContextAccessor.HttpContext ?? throw new InvalidOperationException());

        // claim information
        [BindProperty] public string OnsCode { get; set; } = string.Empty;
        [BindProperty] public string BaseLocalAuthorityName { get; set; } = string.Empty;
        [BindProperty] public int BaseLocalAuthorityId { get; set; }

        // page values
        [BindProperty] public string SoiVersion { get; set; } = string.Empty;
        [BindProperty] public DateTime DatePublished { get; set; }
        [BindProperty] public string SoiLink { get; set; } = string.Empty;

        // error flags
        [BindProperty] public bool SoiVersionHasError { get; set; }
        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;
        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;

        public async Task OnGet()
        {
            logger.LogInformation("{PageName} -  OnGet", PageName);

            await Task.Run(InitialisePage);

            ApplyOriginalData();
        }

        public async Task<IActionResult> OnPostContinue()
        {
            logger.LogInformation("{PageName} -  OnPostContinue", PageName);

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
                Status = SoiStatusV2.ToBeAssessed,
                VersionNumber = SoiVersion,
                PublishedDate = DatePublished,
                StatementOfIntentLink = SoiLink,
                Category = SoiCategory.Incomplete,
                CanSubmit = false,
                DesignatedLas = []
            };

            TempData.Put(TempDataKeys.StatementOfIntentData, request);

            return RedirectToPage(LafPages.DatePublishedView.ROUTE);
        }

        private void ApplyOriginalData()
        {
            // get the prior data values
            var originalRequest = TempData.Get<Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest>(TempDataKeys.StatementOfIntentData);

            if (originalRequest is null) return;

            if (originalRequest.OnsCode != null) OnsCode = originalRequest.OnsCode;
            if (originalRequest.VersionNumber != null) SoiVersion = originalRequest.VersionNumber;
            DatePublished = originalRequest.PublishedDate;
            if (originalRequest.StatementOfIntentLink != null) SoiLink = originalRequest.StatementOfIntentLink;

            TempData.Put(TempDataKeys.StatementOfIntentData, originalRequest);
        }

        private void InitialisePage()
        {
            if (Claims is null)
            {
                DisplayMessage = Constants.UserClaimErrorText;
                logger.LogError(Constants.UserClaimErrorText);
                return;
            }

            if (Claims.BaseLocalAuthority is null)
            {
                DisplayMessage = Constants.UserClaimErrorText;
                logger.LogError(Constants.UserClaimErrorText);
                return;
            }

            if (Claims.BaseLocalAuthority.OnsCode is null)
            {
                DisplayMessage = Constants.UserClaimErrorText;
                logger.LogError(Constants.UserClaimErrorText);
                return;
            }

            if (Claims.BaseLocalAuthority.Name is null)
            {
                DisplayMessage = Constants.UserClaimErrorText;
                logger.LogError(Constants.UserClaimErrorText);
                return;
            }

            OnsCode = Claims.BaseLocalAuthority.OnsCode;
            BaseLocalAuthorityName = Claims.BaseLocalAuthority.Name;
            BaseLocalAuthorityId = Claims.BaseLocalAuthority.ExternalUserLocalAuthorityId;
        }

        private bool ValidBasePage()
        {
            if (string.IsNullOrEmpty(SoiVersion))
            {
                SoiVersionHasError = true;
                DisplayMessage = Constants.VERSION_NUMBER_IS_MANDATORY;
                return false;
            }

            if (SoiVersion.Contains(' '))
            {
                SoiVersionHasError = true;
                DisplayMessage = "Version must not contain spaces.";
                return false;
            }

            if (SoiVersionIsInvalid(SoiVersion))
            {
                SoiVersionHasError = true;
                DisplayMessage = Constants.VERSION_NUMBER_IS_INVALID_MESSAGE;
                return false;
            }

            return true;
        }

        private static bool SoiVersionIsInvalid(string soiVersion)
        {
            foreach (char c in soiVersion)
            {
                if ((!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_') || char.IsWhiteSpace(c))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

