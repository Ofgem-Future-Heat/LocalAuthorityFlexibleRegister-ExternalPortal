using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem.OneLogin.SharedLibrary.Services;

public interface ICustomClaims
{
    Task<IEnumerable<Claim>> GetClaimsAsync(TokenValidatedContext tokenValidatedContext, User? externalUser);
}