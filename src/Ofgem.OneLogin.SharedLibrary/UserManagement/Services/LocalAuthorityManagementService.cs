using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ofgem.API.LAF.LocalAuthorityManagement.Application;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem.OneLogin.SharedLibrary.UserManagement.Services;

public class LocalAuthorityManagementService(
    HttpClient httpClient,
    ILogger<LocalAuthorityManagementService> logger)
    : ILocalAuthorityManagementService
{
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<List<LocalAuthority>?> GetAssociatedLocalAuthoritiesAsync(User externalUser)
    {
        if (externalUser.ExternalUserLocalAuthorities is null) return null;

        try
        {
            var baseLocalAuthority
                = externalUser.ExternalUserLocalAuthorities.SingleOrDefault(x => x.IsBaseLocalAuthority == true);

            if (baseLocalAuthority != null)
            {
                var response = await httpClient.GetAsync($"api/LocalAuthorities/{baseLocalAuthority.OnsCode}/localauthorities");

                var responseData = await response.Content.ReadAsStringAsync();

                var returnedLocalAuthorities = JsonConvert.DeserializeObject<List<LocalAuthority>>(responseData);

                return returnedLocalAuthorities;
            }

        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetUserLocalAuthorities, "Getting associated Local Authorities Failed. {Message}", ex.Message);
            throw;
        }

        return null;
    }
}