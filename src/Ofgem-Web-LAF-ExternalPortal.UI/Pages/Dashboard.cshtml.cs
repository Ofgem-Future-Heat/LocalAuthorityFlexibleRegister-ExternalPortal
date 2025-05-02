using ChoETL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem_Web_LAF_ExternalPortal.Services;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem_Web_LAF_ExternalPortal.Models;
using Ofgem.LAF.SharedLibrary.Enums;

namespace Ofgem_Web_LAF_ExternalPortal.Pages
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]

    public class DashboardModel(
        ILogger<DashboardModel> logger,
        IDeclarationManagementService declarationManagementService,
        IHttpClientFactory httpClientFactory)
        : PageModel
    {



        public Models.Claims? Claims { get; set; }

        [BindProperty] public bool FilterByAddress { get; set; } = true;

        [BindProperty] public string SearchTerm { get; set; } = string.Empty;

        [BindProperty] public Models.DeclarationFilter Filter { get; set; } = new();

        [BindProperty] public List<Models.Declaration>? Declarations { get; set; }

        [BindProperty] public int DeclarationCount { get; set; }
        [BindProperty] public int DeclarationErrorCount { get; set; }
        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public string UploadDetailedInformation { get; set; } = string.Empty;
        [BindProperty] public bool ShowNotification { get; set; }
        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;
        [BindProperty] public bool HasProcessingDeclarationsMessage { get; set; }

        [BindProperty] public bool HasAwaitingDecisionDeclarationsMessage { get; set; }
        [BindProperty] public string DeclarationsDataDescription { get; set; } = string.Empty;
        [BindProperty] public Models.PagePagination Pagination { get; set; } = new();
        [BindProperty] public int CurrentPage { get; set; }
        [BindProperty] public int TotalRecords { get; set; }

        // Date pickers
        [BindProperty] public List<SelectListItem> DateDays { get; set; } = [];
        [BindProperty] public List<SelectListItem> DateMonths { get; set; } = [];
        [BindProperty] public List<SelectListItem> DateFromYears { get; set; } = [];
        [BindProperty] public List<SelectListItem> DateToYears { get; set; } = [];


        public async Task OnGet()
        {
            logger.LogLafInformation(LogEvents.GetDeclarations);

            Claims = new Models.Claims(HttpContext);

            TempData.Remove(TempDataKeys.DashboardFilterData);

            SetupFilterData();

            await FilterData();
        }


        public async Task OnGetSuccessfulUpload(int declarationCount, int declarationErrorCount,
            string detailedUploadMessage = "",
            string displayMessage = "")
        {
            logger.LogInformation("Dashboard - OnGetSuccessfulUpload");

            await GetUploadStatusNotification();

            await GetUploadAwaitingDecisionStatusNotification();

            DeclarationCount = declarationCount;
            UploadDetailedInformation = detailedUploadMessage;
            DeclarationErrorCount = declarationErrorCount;

            ShowNotification = true;

            DisplayMessage = displayMessage;

            // clear any stored filter detail
            TempData.Remove(TempDataKeys.DashboardFilterData);

            SetupFilterData();

            await FilterData();
        }

        public async Task OnPostApplyFilters(bool filterByAddress, string searchTerm)
        {
            logger.LogLafInformation(LogEvents.GetDeclarations);

            Claims = new Models.Claims(HttpContext);

            InitialiseDateDropdowns();

            if (filterByAddress)
            {
                Filter.Address = searchTerm;
                Filter.Urn = string.Empty;
            }
            else
            {
                Filter.Urn = searchTerm;
                Filter.Address = string.Empty;
            }

            CurrentPage = 1;
            Filter.PageIndex = 1;

            if (FilterDatesAreValid())
            {
                await FilterData();
            }
            else
            {
                Declarations = [];
            }
        }


        public async Task OnPostApplySorting()
        {
            logger.LogInformation("Dashboard - OnPostApplySorting");

            Claims = new Models.Claims(HttpContext);

            InitialiseDateDropdowns();

            CurrentPage = 1;
            Filter.PageIndex = 1;

            if (FilterDatesAreValid())
            {
                await FilterData();
            }
            else
            {
                Declarations = [];
            }
        }

        public async Task OnPostApplyPagination(string id)
        {
            logger.LogLafInformation(LogEvents.GetDeclarations);

            Claims = new Models.Claims(HttpContext);

            InitialiseDateDropdowns();

            SetPageIndexes(id);

            if (FilterDatesAreValid())
            {
                await FilterData();
            }
            else
            {
                Declarations = [];
            }
        }

        public Task<IActionResult> OnPostClearFilters()
        {
            logger.LogLafInformation(LogEvents.GetDeclarations);

            Claims = new Models.Claims(HttpContext);

            // clear any stored filter detail
            TempData.Remove(TempDataKeys.DashboardFilterData);

            return Task.FromResult<IActionResult>(RedirectToPage(LafPages.Dashboard.ROUTE));
        }

        public async Task<IActionResult> OnPostDownloadSelectedAsync()
        {
            logger.LogInformation("Dashboard - OnPostApplyFilters");

            TempData.Put(TempDataKeys.DashboardFilterData, Filter);
            TempData.Put(TempDataKeys.DashboardDeclarationCount, new Tuple<int>(TotalRecords));

            DisplayMessage = ErrorMessagesExternalSite.DashBoard.FileDownloaded;
            ShowNotification = true;
            var results
                = await declarationManagementService.GetDownloadDataAsync(
                    (
                        from declaration in Declarations
                        where declaration.Selected
                        select declaration.Urn
                    )
                    .ToList());

            if (results is { Count: 0 })
            {
                DisplayMessage = ErrorMessagesExternalSite.DashBoard.FailedToDownload;
            }

            var ms = new MemoryStream();

            using (var parser = new ChoCSVWriter<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>(ms))
            {
                parser.Write(results);
            }

            ms.Position = 0; //reset stream

            return File(ms, "text/csv", "Dashboard_Download.csv");
        }



        private void SetupFilterData()
        {
            InitialiseDateDropdowns();

            var localFilter = TempData.Get<Models.DeclarationFilter>(TempDataKeys.DashboardFilterData);

            if (localFilter is null)
            {
                SetFilterDefaults();
                ResetFilterRoutes();
                Filter.LAs = CreateLaList();
            }
            else
            {
                TempData.Put(TempDataKeys.DashboardFilterData, localFilter);
                Filter = localFilter;
                CurrentPage = Filter.PageIndex;
            }
        }

        private void ResetFilterRoutes()
        {
            // reset the filter routes as this is the 1st time on the page we want to show all routes
            // but the UI needs to have the shown as unchecked
            Filter.Route1 = false;
            Filter.Route2 = false;
            Filter.Route3 = false;
            Filter.Route4 = false;
        }

        private void SetFromDateFieldValue()
        {
            var minimumDateTime = DateTime.Parse(Filter.DateFromMin, new CultureInfo("en-GB"));

            var dateFrom = DateTime.Today.AddYears(-1).AddDays(1);
            var dateTo = DateTime.Today;

            Filter.DateFromDay = dateFrom.Day;
            Filter.DateFromMonth = dateFrom.Month;
            Filter.DateFromYear = dateFrom.Year;

            Filter.DateToDay = dateTo.Day;
            Filter.DateToMonth = dateTo.Month;
            Filter.DateToYear = dateTo.Year;

            if (dateFrom < minimumDateTime)
            {
                Filter.DateFromDay = minimumDateTime.Day;
                Filter.DateFromMonth = minimumDateTime.Month;
                Filter.DateFromYear = minimumDateTime.Year;
            }

            var dateDiff = dateTo.Subtract(dateFrom);

            if (dateDiff.TotalDays > Models.DeclarationFilter.DateDaysDifference)
            {
                dateFrom = dateTo.AddYears(-1).AddDays(1);
                Filter.DateFromDay = dateFrom.Day;
                Filter.DateFromMonth = dateFrom.Month;
                Filter.DateFromYear = dateFrom.Year;
            }
        }

        private bool FilterDatesAreValid()
        {
            var ukCulture = new CultureInfo("en-GB");

            var dateFromValid = DateTime.TryParse($"{Filter.DateFromDay}/{Filter.DateFromMonth}/{Filter.DateFromYear}", ukCulture, DateTimeStyles.None, out var dateFrom);

            if (!dateFromValid)
            {
                Filter.DateErrorText = "From date is invalid";
                Filter.FromDateHasError = true;
                return false;
            }

            var dateToValid = DateTime.TryParse($"{Filter.DateToDay}/{Filter.DateToMonth}/{Filter.DateToYear}", ukCulture, DateTimeStyles.None, out var dateTo);

            if (!dateToValid)
            {
                Filter.DateErrorText = "To date is invalid";
                Filter.ToDateHasError = true;
                return false;
            }
            
            if (dateFrom >= dateTo)
            {
                Filter.DateErrorText = ErrorMessagesExternalSite.DashBoard.FromDateMustBeEarlierThanTheToDate;
                Filter.DateHasError = true;
                return false;
            }

            var dateDiff = dateTo.Subtract(dateFrom);
            if (dateDiff.TotalDays > Models.DeclarationFilter.DateDaysDifference)
            {
                Filter.DateErrorText = "Invalid date. Ensure search is within a 1 year window.";
                Filter.DateHasError = true;
                return false;
            }

            return true;
        }

        private List<Models.LocalAuthorityVm> CreateLaList()
        {
            var laList = new List<Models.LocalAuthorityVm>();

            try
            {
                foreach (var localAuthority in Claims?.LaList ?? [])
                {
                    laList.Add(new Models.LocalAuthorityVm()
                    {
                        Name = localAuthority.Name,
                        OnsCode = localAuthority.OnsCode,
                        IsBaseLocalAuthority = false,
                        IsSelected = false
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard - CreateLaList");
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.AnIssueOccurredRetrievingData;
            }
            return laList;
        }

        private void SetPageIndexes(string id)
        {
            var isNumeric = int.TryParse(id, out var index);

            if (!isNumeric) return;

            switch (index)
            {
                case Models.PagePagination.PREVIOUS_PAGE_VALUE:
                    Filter.PageIndex = CurrentPage;
                    Filter.PageIndex--;
                    CurrentPage = Filter.PageIndex;
                    break;

                case Models.PagePagination.NEXT_PAGE_VALUE:
                    Filter.PageIndex = CurrentPage;
                    Filter.PageIndex++;
                    CurrentPage = Filter.PageIndex;
                    break;

                default:
                    Filter.PageIndex = index;
                    CurrentPage = index;
                    break;
            }
        }

        private void SetFilterDefaults()
        {
            CurrentPage = 1;
            Filter.PageIndex = 1;

            // by default set both selected so all declarations are returned
            Filter.SubmissionErrors = false;
            Filter.Submitted = false;

            // Set a default sort order
            Filter.SortOrderId = Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.SubmittedStatusNewestFirst
                .ToString();

            SetFromDateFieldValue();
        }

        private async Task FilterData()
        {
            try
            {

                var declarationFilterRequest = new Ofgem.LAF.SharedLibrary.Models.DeclarationFilter
                {
                    DateFrom =
                        new DateTime(Filter.DateFromYear, Filter.DateFromMonth, Filter.DateFromDay)
                        .ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT),
                    DateTo =
                        new DateTime(Filter.DateToYear, Filter.DateToMonth, Filter.DateToDay).AddDays(1)
                        .ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATE_ONLY_FORMAT),
                    LAs = GetSelectedLaList().ToArray(),
                    Status = GetSelectedStatusCodes().ToArray(),
                    Route1 = Filter.Route1,
                    Route2 = Filter.Route2,
                    Route3 = Filter.Route3,
                    Route4 = Filter.Route4,
                    RecordsPerPage = Constants.DECLARATIONS_PAGE_SIZE,
                    PageIndex = Filter.PageIndex,
                    SortId = Filter.SortOrderId == null ? DeclarationSortOrder.SortByStatus : (Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder)Enum.Parse(
                        typeof(Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder), Filter.SortOrderId)
                };

                if (FilterByAddress)
                {
                    declarationFilterRequest.Address = SearchTerm;
                }
                else
                {
                    declarationFilterRequest.Urn = SearchTerm;
                }

                var result = await declarationManagementService.GetPagedDeclarationsAsync(declarationFilterRequest);

                Declarations = result.Declarations.ToList();

                SetSafeToShowFlags();

                DeclarationCount = Declarations.Count;

                DeclarationsDataDescription = ErrorMessagesExternalSite.DashBoard.DeclarationShownInSubmittedDateAscendingOrder;

                CreatePagination(result.RowCount, result.CurrentPage, result.PageCount);

                TotalRecords = result.RowCount;

                // store these in case the next post has validation errors, we will need these values
                TempData.Put(TempDataKeys.DashboardFilterData, Filter);
                TempData.Put(TempDataKeys.DashboardDeclarationCount, new Tuple<int>(TotalRecords));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard - FilterData");
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.AnIssueOccurredRetrievingData;
            }
        }

        private void SetSafeToShowFlags()
        {
            if (Claims is null) return;
            if (Declarations is null) return;

            var acceptableLaCodes = string.Join(", ", Claims.AsArrayOfStrings());

            foreach (var declaration in Declarations)
            {
                declaration.SafeToShow = acceptableLaCodes.Contains(declaration.LAAreaCode ?? string.Empty);
            }
        }

        private List<Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus> GetSelectedStatusCodes()
        {
            List<Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus> selectedStatus = [];
            if (!Filter.Submitted && !Filter.SubmissionErrors)
            {
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Accepted);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Rejected);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.AwaitingSignOff);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Withdrawn);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.FailedCoreChecks);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.FailedSoiChecks);
            }
            else
            {
                if (Filter.Submitted)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Accepted);
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Rejected);
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.AwaitingSignOff);
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.Withdrawn);
                }

                if (Filter.SubmissionErrors)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.FailedCoreChecks);
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.DeclarationStatus.FailedSoiChecks);
                }
            }

            return selectedStatus;
        }

        private List<string> GetSelectedLaList()
        {
            List<string> selectedLaItems = [];

            if (!Filter.LAs.Where(x => x.IsSelected).Any())
            {
                // none are selected.
                // so we want to get ALL the LSs
                foreach (var listItem in Filter.LAs)
                {
                    selectedLaItems.Add(listItem.OnsCode);
                }
            }
            else
            {
                foreach (var listItem in Filter.LAs)
                {
                    if (listItem.IsSelected && listItem.OnsCode != null)
                        selectedLaItems.Add(listItem.OnsCode);
                }
            }
            return selectedLaItems;
        }

        private void CreatePagination(int rowCount, int currentPage, int resultPageCount)
        {
            Filter.PageIndex = currentPage;

            Pagination = new Models.PagePagination(rowCount, currentPage, resultPageCount);
        }

        private async Task GetUploadStatusNotification()
        {
            HasProcessingDeclarationsMessage = false;
            var httpClient = httpClientFactory.CreateClient(DeclarationApi.ApiName);
            try
            {
                var httpResponseMessage = await httpClient.GetAsync(DeclarationApi.RouteUploadsProcessing);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.Upload>>();

                    if (result is { Count: > 0 })
                    {
                        HasProcessingDeclarationsMessage = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard - GetUploadStatusNotification");
                DisplayMessage = ErrorMessagesExternalSite.DashBoard.AnIssueOccurredRetrievingUploadsProcessingStatus;
            }
        }

        private async Task GetUploadAwaitingDecisionStatusNotification()
        {
            HasAwaitingDecisionDeclarationsMessage = false;
            var httpClient = httpClientFactory.CreateClient(DeclarationApi.ApiName);
            try
            {
                var httpResponseMessage =
                    await httpClient.GetAsync(DeclarationApi.RouteUploadsAwaitingDecision);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.Upload>>();

                    if (result is { Count: > 0 })
                    {
                        HasAwaitingDecisionDeclarationsMessage = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard - GetUploadAwaitingDecisionStatusNotification");
                DisplayMessage = ErrorMessagesExternalSite.DashBoard.AnIssueOccurredRetrievingUploadsAwaitingDecisionStatus;
            }
        }

        private void InitialiseDateDropdowns()
        {
            DateDays = Enumerable.Range(1, 31).Select(day => new SelectListItem
            {
                Value = day.ToString(),
                Text = day.ToString()
            }).ToList();

            DateMonths = Enumerable.Range(1, 12).Select(month => new SelectListItem
            {
                Value = month.ToString(),
                Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)[..3]
            }).ToList();

            DateFromYears = Enumerable.Range(2023, 5).Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            }).ToList();

            DateToYears = Enumerable.Range(2024, 5).Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            }).ToList();

        }
    }
}