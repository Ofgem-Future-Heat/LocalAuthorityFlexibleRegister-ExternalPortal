using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.Services;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem.OneLogin.SharedLibrary.Models;

public class CustomClaims : ICustomClaims
{
    public async Task<IEnumerable<Claim>> GetClaimsAsync(TokenValidatedContext tokenValidatedContext, User? externalUser)
    {
        var getClaimsTask = Task.Run(() =>
        {
            string? providerId = tokenValidatedContext.Principal?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (providerId == null || externalUser == null || externalUser.ExternalUserLocalAuthorities == null || externalUser.UserId == Guid.Empty)
            {
                return Enumerable.Empty<Claim>();
            }

            string userLaList = JsonSerializer.Serialize(externalUser.ExternalUserLocalAuthorities);
            IEnumerable<Claim> claims = new List<Claim>
            {
                new Claim(ClaimsConstants.UserLocalAuthorities, $"{userLaList}"),
                new Claim(ClaimsConstants.ExternalUserId, $"{externalUser.UserId}")
            };

            return claims;
        });

        return await getClaimsTask;
    }
}