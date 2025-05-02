using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem.OneLogin.SharedLibrary.Services;

public interface IOidcService
{
    Task<Token?> GetToken(OpenIdConnectMessage? openIdConnectMessage,bool includeCodeVerifierInRequest);


    /// <summary>
    /// Populate account claims from OneLogin
    /// </summary>
    /// <param name="tokenValidatedContext">Validated token context</param>
    /// <returns></returns>
    Task PopulateAccountClaims(TokenValidatedContext tokenValidatedContext);
        
    /// <summary>
    /// Populate custom claims from Ofgem
    /// </summary>
    /// <param name="tokenValidatedContext">Validated token context</param>
    /// <param name="externalUser">External user</param>
    /// <returns></returns>
    Task PopulateCustomClaims(TokenValidatedContext tokenValidatedContext, User? externalUser);
}