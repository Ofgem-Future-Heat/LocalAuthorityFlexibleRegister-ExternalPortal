using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Models;
using System.Net;

namespace Ofgem_Web_LAF_ExternalPortal.Services;


public interface IUserManagementServiceUI
{
    Task<(bool Success, string ErrorMessage)> DeactivateExternalUserAsync(string userId);
    Task<(bool Success, string ErrorMessage)> ReactivateExternalUserAsync(string userId);

    Task<List<Ofgem.LAF.SharedLibrary.Models.User?>> GetManagingLaOfficers(string onsCode);
    Task<Ofgem.LAF.SharedLibrary.Models.User?> GetDesignatedAuthorisedSignatory(string onsCode);

    Task<(Ofgem.LAF.SharedLibrary.Models.User? DasUser, bool Found, string ErrorMessage)> AuthorisedSignatoryExistsAsync(string homeBaseLocalAuthority, string selectedLa);

    Task<(bool Success, string ErrorMessage)> CreateExternalUserAsync(
        Ofgem.LAF.SharedLibrary.Models.User externalUser,
        Ofgem.LAF.SharedLibrary.Models.LocalAuthority baseLocalAuthority);

    Task<(bool Success, string ErrorMessage)> UpdateExternalUserAsync(
        Ofgem.LAF.SharedLibrary.Models.User externalUser,
        Ofgem.LAF.SharedLibrary.Models.LocalAuthority baseLocalAuthority);

    Task<Ofgem.LAF.SharedLibrary.Models.User?> GetExternalUserAsync(string userId);

    Task<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.User?>> GetFilteredUsers(
        FilterExternalUsers externalUsersFilterRequest);

    Task<List<AnnouncementView>> GetPublishedAnnouncementsAsync();
}


public class UserManagementServiceUI(HttpClient httpClient, ILogger<UserManagementServiceUI> logger)
    : IUserManagementServiceUI
{
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<List<Ofgem.LAF.SharedLibrary.Models.User?>> GetManagingLaOfficers(string onsCode)
    {
        logger.LogLafInformation(LogEvents.GetUserEmails);

        var users = new List<Ofgem.LAF.SharedLibrary.Models.User?>();

        try
        {
            var httpResponseMessage = await httpClient.GetAsync($"/api/external-user/{onsCode}/managing-la-officers");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.User?>>();

                return result ?? users;
            }
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDeclaration);
            throw;
        }

        return users;
    }

    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<Ofgem.LAF.SharedLibrary.Models.User?> GetDesignatedAuthorisedSignatory(string onsCode)
    {
        logger.LogLafInformation(LogEvents.GetUserEmails);
        logger.LogLafInformation(LogEvents.GetUserEmails);

        var user = new Ofgem.LAF.SharedLibrary.Models.User();

        try
        {
            var httpResponseMessage = await httpClient.GetAsync($"/api/external-user/{onsCode}/authorised-signatory");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.User?>();

                return result ?? user;
            }
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDeclaration);
            throw;
        }

        return user;
    }

    private const string GenericDataIssueMessage = "Error occurred when creating external user";


    public async Task<(Ofgem.LAF.SharedLibrary.Models.User? DasUser, bool Found, string ErrorMessage)> AuthorisedSignatoryExistsAsync(string homeBaseLocalAuthority, string selectedLa)
    {
        string message;

        // check that there is ONLY 1 Dedicated Authorised Signatory
        try
        {
            var targetAuthority = string.Empty;

            if (!string.IsNullOrEmpty(homeBaseLocalAuthority))
            {
                targetAuthority = homeBaseLocalAuthority;
            }

            if (!string.IsNullOrEmpty(selectedLa))
            {
                targetAuthority = selectedLa;
            }

            var httpResponseMessage =
                await httpClient.GetAsync(
                    $"{UserApi.RouteGetExternalUser}/{targetAuthority}/{UserApi.RouteExistingAuthorisedSignatory}");

            message = string.Empty;

            if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound) return (null, false, message);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content
                    .ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.User?>();

                if (result is not null)
                {
                    return (result, true, message);
                }
            }
            else
            {
                var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

                if (problem?.Detail != null)
                {
                    message = problem.Detail;
                    return (null, true, message);
                }
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, ex.Message);
        }

        message = GenericDataIssueMessage;
        return (null, true, message);
    }

    public async Task<(bool Success, string ErrorMessage)> CreateExternalUserAsync(
        Ofgem.LAF.SharedLibrary.Models.User externalUser,
        Ofgem.LAF.SharedLibrary.Models.LocalAuthority baseLocalAuthority)
    {
        var message = string.Empty;

        try
        {
            externalUser.IsExternal = true;

            externalUser.ExternalUserLocalAuthorities = [];
            externalUser.ExternalUserLocalAuthorities
                .Add(new Ofgem.LAF.SharedLibrary.Models.ExternalUserLocalAuthority()
                {
                    OnsCode = baseLocalAuthority.OnsCode,
                    Name = baseLocalAuthority.Name,
                    IsBaseLocalAuthority = true
                });


            var httpResponseMessage = await httpClient.PostAsJsonAsync(UserApi.RouteAddExternalUser, externalUser);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return (true, message);
            }

            var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem?.Detail != null)
            {
                message = problem.Detail;
                return (false, message);
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, ex.Message);
        }

        message = GenericDataIssueMessage;
        return (false, message);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateExternalUserAsync(
        Ofgem.LAF.SharedLibrary.Models.User externalUser,
        Ofgem.LAF.SharedLibrary.Models.LocalAuthority baseLocalAuthority)
    {
        var message = string.Empty;

        try
        {
            externalUser.ExternalUserLocalAuthorities = [];
            externalUser.ExternalUserLocalAuthorities
                .Add(new Ofgem.LAF.SharedLibrary.Models.ExternalUserLocalAuthority()
                {
                    OnsCode = baseLocalAuthority.OnsCode,
                    Name = baseLocalAuthority.Name,
                    IsBaseLocalAuthority = true
                });


            var httpResponseMessage = await httpClient.PutAsJsonAsync(UserApi.RouteEditExternalUser, externalUser);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return (true, message);
            }

            var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem?.Detail != null)
            {
                message = problem.Detail;
                return (false, message);
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, ex.Message);
        }

        message = GenericDataIssueMessage;
        return (false, message);
    }

    public async Task<Ofgem.LAF.SharedLibrary.Models.User?> GetExternalUserAsync(string userId)
    {

        try
        {
            var httpResponseMessage = await httpClient.GetAsync($"{UserApi.Route}/{userId}");

            if (!httpResponseMessage.IsSuccessStatusCode) return null;

            var result =
                httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.User?>();

            return result.Result ?? null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.User?>> GetFilteredUsers(FilterExternalUsers externalUsersFilterRequest)
    {
        var userFilter = new Ofgem.LAF.SharedLibrary.Models.ExternalUserFilter()
        {
            Filter = externalUsersFilterRequest.Filter,
            PageIndex = externalUsersFilterRequest.PageIndex,
            RecordsPerPage = int.Parse(externalUsersFilterRequest.RecordsPerPage),
            OnsCode = externalUsersFilterRequest.OnsCode

        };

        var httpResponseMessage =
            await httpClient.PostAsJsonAsync(Services.UserApi.RouteGetFilteredExternalUser, userFilter);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.User>>();

            return result;
        }

        return new Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.User?>();
    }

    public async Task<(bool Success, string ErrorMessage)> DeactivateExternalUserAsync(
        string userId)
    {
        var message = string.Empty;

        try
        {
            var httpResponseMessage = await httpClient.PutAsync($"{UserApi.RouteDeactivateExternalUser}/{userId}", null);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return (true, message);
            }

            var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem?.Detail != null)
            {
                message = problem.Detail;
                return (false, message);
            }
        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, ex.Message);
        }

        message = GenericDataIssueMessage;
        return (false, message);
    }
    public async Task<(bool Success, string ErrorMessage)> ReactivateExternalUserAsync(
        string userId)
    {
        var message = string.Empty;

        try
        {
            var httpResponseMessage = await httpClient.PutAsync($"{UserApi.RouteReactivateExternalUser}/{userId}", null);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return (true, message);
            }

            var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

            if (problem?.Detail != null)
            {
                message = problem.Detail;
                return (false, message);
            }
        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, ex.Message);
        }

        message = GenericDataIssueMessage;
        return (false, message);
    }

    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<List<AnnouncementView>> GetPublishedAnnouncementsAsync()
    {
        List<AnnouncementView> announcements = [];
        try
        {
            var httpResponseMessage =
                await httpClient.GetAsync(UserApi.RouteGetPublishedAnnouncements);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var errorMessage = await httpResponseMessage.Content.ReadAsStringAsync();

                logger.LogError("Getting Published Announcements Failed. {Message}", errorMessage);

                return announcements;
            }

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<Ofgem.LAF.SharedLibrary.Models.Announcement>>();

            if (result == null) return announcements;

            announcements = [];

            foreach (var item in result)
            {
                if (item is null) continue;

                announcements.Add(AnnouncementView.MapFromDtoDeclaration(item));
            }

            return announcements;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Getting Published Announcements Failed. {Message}", ex.Message);
        }

        return announcements;
    }
}