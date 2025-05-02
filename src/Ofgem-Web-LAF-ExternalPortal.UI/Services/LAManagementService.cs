using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using System.Globalization;
using Ofgem.LAF.SharedLibrary.Enums;
using static Ofgem_Web_LAF_ExternalPortal.Models.StatementOfIntent;

namespace Ofgem_Web_LAF_ExternalPortal.Services;

public interface ILaManagementService
{
    Task<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>?> GetLocalAuthoritiesAsync();

    Task<Models.SoiDashboardView> GetPagedSoiListAsync(Ofgem.LAF.SharedLibrary.Models.SoiFilter soiListFilter);
    Task<(Ofgem.LAF.SharedLibrary.Models.StatementOfIntent? statementOfIntent, bool Success, string? ErrorMessage)> CreateSoiAsync(StatementOfIntentCreateRequest? request);
    Task<Ofgem.LAF.SharedLibrary.Models.LocalAuthority?> GetByOnsCodeAsync(string onsCode);
    Task<Models.StatementOfIntent?> GetSoiById(Guid statementOfIntentId);
    Task<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent?> GetSoiByOns(string onsCode, string publishedDate);
}


public class LaManagementService(HttpClient httpClient,
    ILogger<LaManagementService> logger)
    : ILaManagementService
{
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>?> GetLocalAuthoritiesAsync()
    {
        try
        {
            List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>? result = [];

            var httpResponseMessage = await httpClient.GetAsync(LocalAuthorityApi.Route);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>>();
            }

            return result ?? [];
        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetLocalAuthorities, $"An issue occurred retrieving local authority data, {ex.Message}");
            return [];
        }
    }

    public async Task<(Ofgem.LAF.SharedLibrary.Models.StatementOfIntent? statementOfIntent, bool Success, string? ErrorMessage)> CreateSoiAsync(StatementOfIntentCreateRequest? request)
    {
        try
        {
            var httpResponseMessage =
                await httpClient.PostAsJsonAsync(LocalAuthorityApi.RouteCreateSoi, request);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent?>();
                return (result, true, null);
            }

            var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem is { Detail: not null })
            {
                return (null, false, problem.Detail);
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetLocalAuthorities, $"An issue occurred retrieving local authority data, {ex.Message}");

        }

        return (null, false, null);
    }

    public async Task<Ofgem.LAF.SharedLibrary.Models.LocalAuthority?> GetByOnsCodeAsync(string onsCode)
    {
        try
        {
            var httpResponseMessage =
                await httpClient.GetAsync(LocalAuthorityApi.RouteGetByOnsCode + onsCode);

            if (httpResponseMessage.IsSuccessStatusCode)
            {

                var la = await httpResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                return la;
            }

            return null;

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetLocalAuthorities,
                $"An issue occurred retrieving local authority data, {ex.Message}");
        }

        return null;
    }

    public async Task<Models.SoiDashboardView> GetPagedSoiListAsync(Ofgem.LAF.SharedLibrary.Models.SoiFilter soiListFilter)
    {
        try
        {
            var returnModel = new Models.SoiDashboardView();

            var httpResponseMessage = await httpClient.PostAsJsonAsync(Services.LocalAuthorityApi.RouteSoiList, soiListFilter);

            if (!httpResponseMessage.IsSuccessStatusCode) return returnModel;

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent>>();

            if (result == null) return returnModel;

            foreach (var item in result.Results)
            {
                returnModel.StatementOfIntents.Add(Models.StatementOfIntent.MapFromDtoStatementOfIntent(item));
            }

            returnModel.CurrentPage = result.CurrentPage;
            returnModel.FirstRowOnPage = result.FirstRowOnPage;
            returnModel.LastRowOnPage = result.LastRowOnPage;
            returnModel.PageCount = result.PageCount;
            returnModel.PageSize = result.PageSize;
            returnModel.RowCount = result.RowCount;

            return returnModel;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetSoi);
            throw;
        }
    }

    public async Task<Models.StatementOfIntent?> GetSoiById(Guid statementOfIntentId)
    {
        try
        {
            var returnModel = new Models.StatementOfIntent();

            var httpResponseMessage =
                await httpClient.GetAsync(LocalAuthorityApi.RouteSoiById + statementOfIntentId);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent>();

                if (result == null) return returnModel;

                returnModel.StatementOfIntentId = result.StatementOfIntentId;
                returnModel.VersionNumber = result.VersionNumber;
                returnModel.SoiLink = result.StatementOfIntentLink;
                returnModel.DateAdded = result.PublishedDate;
                returnModel.SoiStatus = result.Status;
                returnModel.ForScheme = result.ForScheme;
                returnModel.DesignatedLAs = result.DesignatedLas?.Select(item => new DesignatedLa
                {
                    LocalAuthorityId = item?.LocalAuthority?.LocalAuthorityId,
                    LocalAuthorityName = item?.LocalAuthority?.Name,
                    OnsCode = item?.LocalAuthority?.OnsCode
                }).ToList() ?? new List<DesignatedLa>();

                var httpLaResponse =
                    await httpClient.GetAsync(LocalAuthorityApi.Route + "/" + result.LocalAuthorityId);

                var laResult = await httpLaResponse.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>();

                if (laResult == null) return returnModel;

                returnModel.LocalAuthority = laResult.Name;

                return returnModel;
            }

            return null;

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetLocalAuthorities,
                $"An issue occurred retrieving statement of intent data, {ex.Message}");
        }

        return null;
    }

    public async Task<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent?> GetSoiByOns(string onsCode, string publishedDate)
    {
        try
        {
            var ukCulture = new CultureInfo("en-GB");
            var isDateValid = DateTime.TryParse(publishedDate, ukCulture, DateTimeStyles.None, out var newPublishDate);

            if (!isDateValid)
            {
                return null;
            }

            string formattedPublishedDate = newPublishDate.ToString("yyyy-MM-dd");

            var httpResponseMessage = await httpClient.GetAsync(LocalAuthorityApi.RouteGetSoiByOns + "/" + onsCode + "/" + formattedPublishedDate);

            if (!httpResponseMessage.IsSuccessStatusCode) return null;

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.StatementOfIntent?>();

            if (result == null) return null;

            return result;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDeclarations);
            throw;
        }
    }
}