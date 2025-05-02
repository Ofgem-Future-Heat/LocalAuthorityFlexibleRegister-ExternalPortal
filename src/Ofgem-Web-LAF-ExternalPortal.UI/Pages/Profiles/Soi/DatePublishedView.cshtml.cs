using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using System.Globalization;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Profiles.Soi
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class DatePublishedViewModel(
        ILogger<DatePublishedViewModel> logger,
        IHttpContextAccessor httpContextAccessor,
        Services.ILaManagementService laManagementService) : SoiPage
    {
        private Services.ILaManagementService LaManagementService { get; set; } = laManagementService;

        private const string PageName = "DatePublishedView";

        public Models.Claims? Claims { get; set; } = new(httpContextAccessor.HttpContext ?? throw new InvalidOperationException());

        // claim information
        [BindProperty] public string OnsCode { get; set; } = string.Empty;
        [BindProperty] public string BaseLocalAuthorityName { get; set; } = string.Empty;
        [BindProperty] public int BaseLocalAuthorityId { get; set; }

        // page values
        [BindProperty] public string SoiVersion { get; set; } = string.Empty;
        [BindProperty] public Models.ThreePartDate? DatePublishedControl { get; set; }
        [BindProperty] public DateTime DatePublished { get; set; }

        // validation values
        public string DateFromMin { get; init; } = Constants.SOI_MINIMUM_DATE;
        public string DateFromMax { get; init; } = DateTime.Now.ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATETIME_END_OF_DAY);

        // error flags
        [BindProperty] public bool DatePublishedHasError { get; set; }
        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;
        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;


        public Task OnGet()
        {
            logger.LogInformation("{PageName} -  OnGet", PageName);

            var request = InitialisePage();

            if (request is null)
            {
                DisplayMessage = Constants.TempDataReloadIssueText;
                logger.LogError(Constants.TempDataReloadIssueText);
            }

            DatePublishedControl = new Models.ThreePartDate(
                source: request?.PublishedDate ?? DateTime.MinValue,
                title: "",
                titleToBeUsedInErrorMessage: "",
                firstHint: "",
                secondHint: "",
                showTheHighlightBar: true);

            return Task.CompletedTask;
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

            if (DatePublishedControl != null)
            {
                DatePublished = new DateTime(DatePublishedControl.Year, DatePublishedControl.Month, DatePublishedControl.Day, 0, 0, 0, DateTimeKind.Utc);
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
                DesignatedLas = []
            };

            TempData.Put(TempDataKeys.StatementOfIntentData, request);

            return RedirectToPage(LafPages.LinkView.ROUTE);
        }

        private bool ValidBasePage()
        {
            if (!ValidateDatePublishedControl())
            {
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

            DatePublished = new DateTime(DatePublishedControl.Year, DatePublishedControl.Month, DatePublishedControl.Day, 0, 0, 0, DateTimeKind.Utc);

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
            if (request.PublishedDate == DateTime.MinValue)
            {
                request.PublishedDate = DateTime.Today;
            }
            else
            {
                DatePublished = request.PublishedDate;
            }

            TempData.Put(TempDataKeys.StatementOfIntentData, request);

            return request;
        }
    }
}

