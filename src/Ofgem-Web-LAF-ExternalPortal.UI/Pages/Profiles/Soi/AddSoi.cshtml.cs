
using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Profiles.Soi
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class AddSoi(
        ILogger<AddSoi> logger,
        IHttpContextAccessor httpContextAccessor,
        Services.ILaManagementService laManagementService)
        : SoiPage
    {
        private const string PageName = "AddSoi";

        public Models.Claims? Claims { get; set; } = new (httpContextAccessor.HttpContext ?? throw new InvalidOperationException());

        // claim information
        [BindProperty] public string OnsCode { get; set; } = string.Empty;
        [BindProperty] public string BaseLocalAuthorityName { get; set; } = string.Empty;
        [BindProperty] public int BaseLocalAuthorityId { get; set; }


        // page values
        [BindProperty] public string SoiVersion { get; set; } = string.Empty;
        [BindProperty] public Models.ThreePartDate? DatePublishedControl { get; set; }
        [BindProperty] public DateTime DatePublished { get; set; }
        [BindProperty] public string SoiLink { get; set; } = string.Empty;


        // validation values
        public string DateFromMin { get; init; } = Constants.SOI_MINIMUM_DATE;
        public string DateFromMax { get; init; } = DateTime.Now.ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATETIME_END_OF_DAY);


        // error flags
        [BindProperty] public bool SoiVersionHasError { get; set; }
        [BindProperty] public bool SoiLinkHasError { get; set; }
        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;
        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;



        private ILogger<AddSoi> Logger { get; set; } = logger;
        private Services.ILaManagementService LaManagementService { get; set; } = laManagementService;


        public async Task OnGet()
        {
            Logger.LogInformation("{PageName} -  OnGet", PageName);

            await Task.Run(InitialisePage);

            ApplyOriginalData();
        }

        private void ApplyOriginalData()
        {
            // get the prior data values
            var originalRequest = TempData.Get<Ofgem.LAF.SharedLibrary.Models.StatementOfIntentCreateRequest>(SoiDataKey);

            if (originalRequest is null) return;

            if (originalRequest.OnsCode != null) OnsCode = originalRequest.OnsCode;
            if (originalRequest.VersionNumber != null) SoiVersion = originalRequest.VersionNumber;
            if (originalRequest.StatementOfIntentLink != null) SoiLink = originalRequest.StatementOfIntentLink;
            DatePublished = originalRequest.PublishedDate;
        }

        public async Task<IActionResult> OnPostAdd()
        {
            Logger.LogInformation("{PageName} -  OnPostAdd", PageName);

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

            TempData.Put(SoiDataKey, request);

            return RedirectToPage(LafPages.AddSoiSummary.ROUTE);
        }

        private void InitialisePage()
        {
            if (Claims is null)
            {
                DisplayMessage = "Unable to determine the users claims";
                Logger.LogError("Unable to determine the users claims");
                return;
            }

            if (Claims.BaseLocalAuthority is null)
            {
                DisplayMessage = "Unable to determine the users base authority";
                Logger.LogError("Unable to determine the users base authority");
                return;
            }

            if (Claims.BaseLocalAuthority.OnsCode is null)
            {
                DisplayMessage = "Unable to determine the users base authority";
                Logger.LogError("Unable to determine the users base authority");
                return;
            }

            if (Claims.BaseLocalAuthority.Name is null)
            {
                DisplayMessage = "Unable to determine the users base authority";
                Logger.LogError("Unable to determine the users base authority");
                return;
            }

            OnsCode = Claims.BaseLocalAuthority.OnsCode;
            BaseLocalAuthorityName = Claims.BaseLocalAuthority.Name;
            BaseLocalAuthorityId = Claims.BaseLocalAuthority.ExternalUserLocalAuthorityId;

            DatePublishedControl = new Models.ThreePartDate(
                source: DateTime.Today,
                title: "Date SOI was published",
                titleToBeUsedInErrorMessage: "Date SOI was published",
                firstHint: "You cannot edit this date after uploading a declaration notification referencing this version of the SOI",
                secondHint: "For example, 25 2 2024",
                showTheHighlightBar: true);

            DatePublished = new DateTime(DatePublishedControl.Year, DatePublishedControl.Month, DatePublishedControl.Day);
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

            if (!ValidateDatePublishedControl())
            {
                return false;
            }

            if (!UrlValidation.IsLinkValid(SoiLink))
            {
                SoiLinkHasError = true;
                DisplayMessage = "SoI link is mandatory and must be a hyperlink.";
                return false;
            }

            return true;
        }

        private bool ValidateDatePublishedControl()
        {
            if (DatePublishedControl is null) return false;

            if (DatePublishedControl.HasErrors())
            {
                DisplayMessage = DatePublishedControl.ErrorMessage;
                return false;
            }

            DatePublished = new DateTime(DatePublishedControl.Year, DatePublishedControl.Month, DatePublishedControl.Day);

            if (DatePublished < Convert.ToDateTime(DateFromMin, CultureInfo.InvariantCulture))
            {
                DisplayMessage = "A valid date must have a correct input for year.";
                DatePublishedControl.HasError = true;
                DatePublishedControl.HasYearError = true;
                DatePublishedControl.ErrorMessage = DisplayMessage;
                return false;
            }

            if (DatePublished > Convert.ToDateTime(DateFromMax, CultureInfo.InvariantCulture))
            {
                DisplayMessage = $"Date published field cannot be after {DateTime.Now:dd/MM/yyyy}";
                DatePublishedControl.HasError = true;
                DatePublishedControl.ErrorMessage = DisplayMessage;
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
