using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ofgem.API.LAF.LocalAuthorityManagement.Application;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem.OneLogin.SharedLibrary.UserManagement.Services;

public class UserManagementService(ILocalAuthorityManagementService localAuthorityManagementService, HttpClient httpClient, ILogger<UserManagementService> logger)
    : IUserManagementService
{
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<User?> SyncExternalUserAsync(string providerId, string emailAddress)
    {
        try
        {
            var request = new SyncExternalUserRequest
            {
                EmailAddress = emailAddress
            };

            var response = await httpClient.PutAsJsonAsync($"api/external-user/{providerId}/sync", request);

            var responseData = await response.Content.ReadAsStringAsync();
            var returnedUser = JsonConvert.DeserializeObject<User>(responseData);

            return returnedUser;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.ExternalUsers, "Synchronization update of external user failed.", emailAddress);
            throw;
        }
    }

    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<User?> SyncExternalUserAsync(TokenValidatedContext tokenValidatedContext)
    {
        try
        {
            var providerId = tokenValidatedContext.Principal?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = tokenValidatedContext.Principal?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            if (providerId == null || email == null) return null;

            var externalUser = await SyncExternalUserAsync(providerId, email);

            if (externalUser is null) return null;

            if (externalUser.UserId == Guid.Empty || externalUser is not { IsActive: true, IsDeleted: false }) return externalUser;

            tokenValidatedContext.Principal?.Identities.First().AddClaim(new Claim(ClaimsConstants.LinkedAccount, "true"));

            tokenValidatedContext.Principal?.Identities.First().AddClaim(new Claim(ClaimsConstants.UserType, externalUser.UserType.ToString()));

            tokenValidatedContext.Principal?.Identities.First().AddClaim(new Claim(ClaimsConstants.FirstName, externalUser.FirstName ?? string.Empty));

            tokenValidatedContext.Principal?.Identities.First().AddClaim(new Claim(ClaimsConstants.LastName, externalUser.LastName ?? string.Empty));

            // now add in the associated local authorities
            var localAuthorities = await localAuthorityManagementService.GetAssociatedLocalAuthoritiesAsync(externalUser);

            foreach (var la in localAuthorities ?? [])
            {
                if (!externalUser.ExternalUserLocalAuthorities.Any(x => x.OnsCode == la.OnsCode))
                {
                    externalUser.ExternalUserLocalAuthorities?.Add(new ExternalUserLocalAuthority
                    {
                        OnsCode = la.OnsCode,
                        Name = la.Name,
                        IsBaseLocalAuthority = false,
                        UserId = externalUser.UserId
                    });
                }
            }

            return externalUser;

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.ExternalUsers, "Synchronization of external user failed.", ex);
            throw;
        }
    }
}