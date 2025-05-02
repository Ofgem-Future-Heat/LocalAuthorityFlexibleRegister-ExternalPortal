using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Ofgem.OneLogin.SharedLibrary.Models;

namespace Ofgem.OneLogin.SharedLibrary.Services;

public interface IJwtSecurityTokenService
{
    string CreateToken(string clientId, string audience, ClaimsIdentity claimsIdentity,
        SigningCredentials signingCredentials);

    JwtToken ReadToken(string token);
}