using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ofgem.API.LAF.LocalAuthorityManagement.Application;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ofgem.OneLogin.SharedLibrary.Services;

internal class JwtSecurityTokenService(ILogger<JwtSecurityTokenService> logger) : IJwtSecurityTokenService
{
    private readonly ILogger<IJwtSecurityTokenService> _logger = logger;

    public string CreateToken(string clientId, string audience, ClaimsIdentity claimsIdentity,
        SigningCredentials signingCredentials)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var value = handler.CreateJwtSecurityToken(clientId, audience, claimsIdentity, DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, signingCredentials);

            return value.RawData;
        }
        catch (Exception ex)
        {
            _logger.LogLafError(LogEvents.JwtSecurityTokenService, "Create Token failed: {Message} ", ex.Message);
            throw;
        }
    }

    public JwtToken ReadToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var value = handler.ReadJwtToken(token);

            return new JwtToken() { Subject = value.Subject, Issuer = value.Issuer, Audience = value.Audiences, IssuedAt = value.IssuedAt };
        }
        catch (Exception ex)
        {
            _logger.LogLafError(LogEvents.JwtSecurityTokenService, "Read Token failed: {Message}", ex.Message);
            throw;
        }
    }
}