using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Services;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.ExternalUsers;

[AutoValidateAntiforgeryToken]
[Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
[Authorize(Policy = ClaimsConstants.LinkedAccount)]
[Authorize(Policy = "MustHaveClaimLocalAuthorities")]
public class ExternalUsersModel(ILogger<ExternalUsersModel> logger,
    IUserManagementServiceUI userService) : PageModel
{
    public Models.Claims? Claims { get; set; }

    [BindProperty(SupportsGet = true)]
    public Models.FilterExternalUsers Filter { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public List<Ofgem.LAF.SharedLibrary.Models.User> ExternalUsers { get; set; } = [];

    [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

    [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;


    [BindProperty] public Models.PagePagination Pagination { get; set; } = new();
    [BindProperty] public int CurrentPage { get; set; }


    public async Task OnGet()
    {
        logger.LogLafInformation(LogEvents.ExternalUsers, "ExternalUsers - OnGet");
        await RefreshData();
    }

    private string GetOnsCodeFromClaims()
    {
        Claims = new Models.Claims(HttpContext);

        if (Claims.BaseLocalAuthority is null) return string.Empty;
        if (Claims.BaseLocalAuthority.OnsCode is null) return string.Empty;

        return Claims.BaseLocalAuthority.OnsCode;
    }

    public async Task OnGetCreateSuccess(string name)
    {
        logger.LogLafInformation(LogEvents.ExternalUsers, "OnGetCreateSuccess");
        await RefreshData();

        DisplayMessage = $"Created external user - {name}";
    }

    public async Task OnGetUpdateSuccess(string name)
    {
        logger.LogLafInformation(LogEvents.ExternalUsers, "OnGetUpdateSuccess");
        await RefreshData();

        DisplayMessage = $"Updated external user - {name}";
    }



    public async Task OnPostApplyPagination(string id)
    {
        SetPageIndexes(id);

        await FilterData();
    }

    private void SetPageIndexes(string id)
    {
        var isNumeric = int.TryParse(id, out int index);

        if (isNumeric)
        {
            if (index == Models.PagePagination.PREVIOUS_PAGE_VALUE)
            {
                Filter.PageIndex = CurrentPage;
                Filter.PageIndex--;
                CurrentPage = Filter.PageIndex;
            }
            else
            {
                if (index == Models.PagePagination.NEXT_PAGE_VALUE)
                {
                    Filter.PageIndex = CurrentPage;
                    Filter.PageIndex++;
                    CurrentPage = Filter.PageIndex;
                }
                else
                {
                    Filter.PageIndex = index;
                    CurrentPage = index;
                }
            }
        }
    }

    public async Task OnPostApplyFilters()
    {
        logger.LogLafInformation(LogEvents.ExternalUsers, "ExternalUsers - OnPostApplyFilters");

        await RefreshData();
    }

    public async Task OnPostClearFilters()
    {
        logger.LogLafInformation(LogEvents.ExternalUsers, "ExternalUsers - OnPostClearFilters");

        Filter.Filter = string.Empty;

        await RefreshData();
    }

    private async Task RefreshData()
    {
        CurrentPage = 1;
        Filter.PageIndex = 1;

        await FilterData();
    }

    private async Task FilterData()
    {
        try
        {
            var externalUsersFilterRequest = new Models.FilterExternalUsers()
            {
                RecordsPerPage = Filter.RecordsPerPage,
                PageIndex = Filter.PageIndex,
                Filter = Filter.Filter,
                OnsCode = GetOnsCodeFromClaims()
            };

            var externalUsers = await userService.GetFilteredUsers(externalUsersFilterRequest);

            ExternalUsers = externalUsers.Results is null
                ? []
                : [.. externalUsers.Results];

            CreatePagination(externalUsers.RowCount, externalUsers.CurrentPage, externalUsers.PageSize, externalUsers.PageCount);
        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers,
                "ExternalUsers - FilterData, Error {Filter}, Message {Message}", Filter, ex.Message);
            DisplayMessage = "An issue occurred retrieving data";
        }
    }

    private void CreatePagination(int rowCount, int currentPage, int pageSize, int resultPageCount)
    {
        Filter.PageIndex = currentPage;
        Filter.RecordsPerPage = pageSize.ToString();

        Pagination = new Models.PagePagination(rowCount, currentPage, resultPageCount);
    }
}