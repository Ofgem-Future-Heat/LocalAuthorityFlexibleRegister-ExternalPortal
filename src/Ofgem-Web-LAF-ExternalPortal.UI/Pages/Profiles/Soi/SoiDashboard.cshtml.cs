using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;
using Microsoft.AspNetCore.Authorization;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Profiles.Soi
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]

    public class SoiDashboardModel(
        ILogger<SoiDashboardModel> logger,
        Services.ILaManagementService laManagementService) : PageModel
    {
        public Models.Claims? Claims { get; set; }

        [BindProperty] public Ofgem.LAF.SharedLibrary.Enums.ExternalUserType ExternalUserType { get; set; }

        [BindProperty] public List<Models.StatementOfIntent>? StatementOfIntents { get; set; }

        [BindProperty] public Models.SoiFilter Filter { get; set; } = new();

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public int StatementOfIntentsCount { get; set; }

        [BindProperty] public string StatementOfIntentsDataDescription { get; set; } = string.Empty;

        [BindProperty] public Models.PagePagination Pagination { get; set; } = new();

        [BindProperty] public int CurrentPage { get; set; }
        [BindProperty] public int TotalRecords { get; set; }

        public async Task OnGet()
        {
            logger.LogLafInformation(LogEvents.GetSoi);

            Claims = new Models.Claims(HttpContext);

            ExternalUserType = Claims.ExternalUserType;

            TempData.Remove(TempDataKeys.StatementOfIntentFilterData);

            SetupFilterData();
            await FilterData();
        }

        public async Task OnPostApplyFilters()
        {
            logger.LogLafInformation(LogEvents.GetSoi, "StatementOfIntentList - OnPostApplyFilters");

            Claims = new Models.Claims(HttpContext);

            SetFilterDefaults();
            Filter.LAs = CreateLaList();
            await FilterData();
        }

        public Task<IActionResult> OnPostClearFilters()
        {
            logger.LogLafInformation(LogEvents.GetSoi, "StatementOfIntentList - OnPostClearFilters");

            Claims = new Models.Claims(HttpContext);

            // clear any stored filter detail
            TempData.Remove(TempDataKeys.StatementOfIntentFilterData);

            return Task.FromResult<IActionResult>(RedirectToPage(LafPages.SOIDashboard.ROUTE));
        }

        public Task<IActionResult> OnPostClearData()
        {
            logger.LogLafInformation(LogEvents.GetSoi, "StatementOfIntentData - OnPostClearData");

            Claims = new Models.Claims(HttpContext);

            // clear any stored Soi Data
            TempData.Remove(TempDataKeys.StatementOfIntentData);

            return Task.FromResult<IActionResult>(RedirectToPage(LafPages.VersionNumberView.ROUTE));
        }

        private void SetupFilterData()
        {
            var localFilter = TempData.Get<Models.SoiFilter>(TempDataKeys.StatementOfIntentFilterData);

            if (localFilter is null)
            {
                SetFilterDefaults();
                ResetFilterStatus();
                Filter.LAs = CreateLaList();
            }
            else
            {
                TempData.Put(TempDataKeys.StatementOfIntentFilterData, localFilter);
                Filter = localFilter;
                CurrentPage = Filter.PageIndex;
            }
        }

        private void SetFilterDefaults()
        {
            CurrentPage = 1;
            Filter.PageIndex = 1;
        }

        private void ResetFilterStatus()
        {
            // reset the filter status as this is the 1st time on the page we want to show all status
            // but the UI needs to have the shown as checked
            Filter.IsPassedAssessment = false;
            Filter.IsToBeAssessed = false;
            Filter.IsFailedAssessment = false;
            Filter.IsAwaitingSignOff = false;
        }

        private List<Models.LocalAuthorityVm> CreateLaList()
        {
            var laList = new List<Models.LocalAuthorityVm>();

            try
            {
                laList.Add(new Models.LocalAuthorityVm()
                {
                    Name = Claims?.BaseLocalAuthority?.Name,
                    OnsCode = Claims?.BaseLocalAuthority?.OnsCode,
                    IsBaseLocalAuthority = true,
                    IsSelected = false
                });


                foreach (var localAuthority in Claims?.LaList ?? [])
                {
                    if (localAuthority.IsBaseLocalAuthority == true)
                    {
                        continue;
                    }

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
                logger.LogError(ex, "SoiDashboard - LaList");
                DisplayMessage = "An issue occurred retrieving data";
            }
            return laList;
        }

        private async Task FilterData()
        {
            try
            {
                var soiFilterRequest = new Ofgem.LAF.SharedLibrary.Models.SoiFilter
                {

                    BaseLocalAuthority = GetBaseLocalAuthority(),
                    LAs = GetSelectedLaList().ToArray(),
                    Status = GetSelectedStatusCodes().ToArray(),
                    RecordsPerPage = Constants.SOI_PAGE_SIZE,
                    PageIndex = Filter.PageIndex,
                };

                var result = await laManagementService.GetPagedSoiListAsync(soiFilterRequest);

                StatementOfIntents = result.StatementOfIntents.ToList();

                StatementOfIntentsCount = StatementOfIntents.Count;

                StatementOfIntentsDataDescription = "Statement of Intents shown in SOI status order";

                CreatePagination(result.RowCount, result.CurrentPage, result.PageCount);

                TotalRecords = result.RowCount;

                // store these in case the next post has validation errors, we will need these values
                TempData.Put(TempDataKeys.StatementOfIntentFilterData, Filter);
                TempData.Put(TempDataKeys.StatementOfIntentCount, new Tuple<int>(TotalRecords));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SOI Dashboard - FilterData");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        private void CreatePagination(int rowCount, int currentPage, int resultPageCount)
        {
            Filter.PageIndex = currentPage;

            Pagination = new Models.PagePagination(rowCount, currentPage, resultPageCount);
        }

        private List<Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2> GetSelectedStatusCodes()
        {
            List<Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2> selectedStatus = [];

            if (Filter is { IsPassedAssessment: false, IsToBeAssessed: false, IsAwaitingSignOff: false, IsFailedAssessment: false })
            {
                // none are selected.
                // so we want to get ALL the Statuses
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.PassedAssessment);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.ToBeAssessed);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.FailedAssessment);
                selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.AwaitingSignOff);
            }
            else
            {
                if (Filter.IsPassedAssessment)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.PassedAssessment);
                }
                if (Filter.IsToBeAssessed)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.ToBeAssessed);
                }
                if (Filter.IsFailedAssessment)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.FailedAssessment);
                }
                if (Filter.IsAwaitingSignOff)
                {
                    selectedStatus.Add(Ofgem.LAF.SharedLibrary.Enums.SoiStatusV2.AwaitingSignOff);
                }
            }

            return selectedStatus;
        }

        private string? GetBaseLocalAuthority()
        {
            string? selectedBaseLaCode = string.Empty;

            var baseLocalAuthority = Filter.LAs.Where(x => x.IsBaseLocalAuthority == true).FirstOrDefault();

            if (baseLocalAuthority is null)
            {
                return selectedBaseLaCode;
            }

            if (Filter.LAs.Where(x => x.IsSelected).Count() == 0)
            {
                // none are selected.
                // so we want to get Base LA
                selectedBaseLaCode = baseLocalAuthority.OnsCode;
            }
            else
            {
                if (baseLocalAuthority.IsSelected && baseLocalAuthority.OnsCode != null)
                    selectedBaseLaCode = baseLocalAuthority.OnsCode;
            }

            return selectedBaseLaCode;
        }


        private List<string> GetSelectedLaList()
        {
            List<string> selectedLaItems = [];

            if (Filter.LAs.Where(x => x.IsSelected).Count() == 0)
            {
                // none are selected.
                // so we want to get ALL the LAs
                foreach (var listItem in Filter.LAs)
                {
                    if (listItem.IsBaseLocalAuthority == true)
                    {
                        continue;
                    }

                    selectedLaItems.Add(listItem.OnsCode);
                }
            }
            else
            {
                foreach (var listItem in Filter.LAs)
                {
                    if (listItem.IsBaseLocalAuthority == false && listItem.IsSelected && listItem.OnsCode != null)
                        selectedLaItems.Add(listItem.OnsCode);
                }
            }
            return selectedLaItems;
        }

        public async Task OnPostApplyPagination(string id)
        {
            logger.LogLafInformation(LogEvents.GetSoi);

            Claims = new Models.Claims(HttpContext);

            SetPageIndexes(id);

            await FilterData();
        }

        private void SetPageIndexes(string id)
        {
            var isNumeric = int.TryParse(id, out int index);

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
    }
}
