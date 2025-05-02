using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.KeyVaultExtensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Ofgem.API.LAF.LocalAuthorityManagement.Application;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.Configuration;
using Ofgem.OneLogin.SharedLibrary.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Ofgem.OneLogin.SharedLibrary.Services;

public class OidcService : IOidcService
{
    private readonly HttpClient _httpClient;
    private readonly IAzureIdentityService _azureIdentityService;
    private readonly IJwtSecurityTokenService _jwtSecurityTokenService;
    private readonly ICustomClaims? _customClaims;
    private readonly GovUkOidcConfiguration _configuration;
    private readonly ILogger<IOidcService> _logger;

    public OidcService(
        HttpClient httpClient,
        IAzureIdentityService azureIdentityService,
        IJwtSecurityTokenService jwtSecurityTokenService,
        GovUkOidcConfiguration configuration,
        ICustomClaims? customClaims,
        ILogger<OidcService> logger)
    {
        _httpClient = httpClient;
        _azureIdentityService = azureIdentityService;
        _jwtSecurityTokenService = jwtSecurityTokenService;
        _customClaims = customClaims;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri(configuration.BaseUrl);
        _logger = logger;

    }

    public async Task<Token?> GetToken(OpenIdConnectMessage? openIdConnectMessage, bool includeCodeVerifierInRequest)
    {
        try
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/token")
            {
                Headers =
             {
                 Accept =
                 {
                     new MediaTypeWithQualityHeaderValue("*/*"),
                     new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded")
                 },
                 UserAgent = {new ProductInfoHeaderValue("GbisMeasurements", "1")}
             },
                Content = new FormUrlEncodedContent(CreateRequestContentItems(openIdConnectMessage, CreateJwtAssertion(), includeCodeVerifierInRequest))
            };

            httpRequestMessage.Content.Headers.Clear();
            httpRequestMessage.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");


            var response = await _httpClient.SendAsync(httpRequestMessage);
            var valueString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Token>(valueString);
        }
        catch (Exception ex)
        {
            _logger.LogLafError(LogEvents.OidcService, "GetToken: {Message}", ex.Message);
            throw;
        }
    }

    public async Task PopulateAccountClaims(TokenValidatedContext tokenValidatedContext)
    {
        try
        {
            if (tokenValidatedContext.TokenEndpointResponse == null || tokenValidatedContext.Principal == null)
            {
                return;
            }

            var accessToken = tokenValidatedContext.TokenEndpointResponse.Parameters["access_token"];
            var jwtToken = _jwtSecurityTokenService.ReadToken(accessToken);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/userinfo")
            {

                Headers =
                {
                    UserAgent = {new ProductInfoHeaderValue("GbisMeasurements", "1")},
                    Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
                }
            };
            var response = await _httpClient.SendAsync(httpRequestMessage);
            var valueString = response.Content.ReadAsStringAsync().Result;
            var content = JsonSerializer.Deserialize<GovUkUser>(valueString);

            if (!IsUserInfoSubClaimValid(content, jwtToken))
            {
                return;
            }

            if (content?.Email != null)
            {
                tokenValidatedContext.Principal.Identities.First().AddClaim(new Claim(ClaimTypes.Email, content.Email));
            }
        }
        catch (Exception ex)
        {
            _logger.LogLafError(LogEvents.OidcService, "PopulateAccountClaims: {Message}", ex.Message);
            throw;
        }

    }

    /// <summary>
    /// User info sub claim must match jwtToken sub
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="jwtToken"></param>
    /// <returns></returns>
    private static bool IsUserInfoSubClaimValid(GovUkUser? userInfo, JwtToken jwtToken)
    {
        return userInfo?.Sub != null && userInfo.Sub!.Equals(jwtToken.Subject);
    }

    public async Task PopulateCustomClaims(TokenValidatedContext tokenValidatedContext, User? externalUser)
    {
        try
        {
            if (externalUser == null ||
                tokenValidatedContext.TokenEndpointResponse == null ||
                tokenValidatedContext.Principal == null ||
                _customClaims == null)
            {
                return;
            }

            tokenValidatedContext.Principal.Identities.First()
                .AddClaims(await _customClaims.GetClaimsAsync(tokenValidatedContext, externalUser));
        }
        catch (Exception ex)
        {
            _logger.LogLafError(LogEvents.OidcService, "PopulateCustomClaims: {Message}", ex.Message);
            throw;
        }
    }

    private string CreateJwtAssertion()
    {
        try
        {
            var jti = Guid.NewGuid().ToString();
            var claimsIdentity = new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("sub", _configuration.ClientId),
                    new Claim("jti", jti)

                });

            var signingCredentials = new SigningCredentials(
                new KeyVaultSecurityKey(_configuration.KeyVaultIdentifier,
                    _azureIdentityService.AuthenticationCallback), "RS512")
            {
                CryptoProviderFactory = new CryptoProviderFactory
                {
                    CustomCryptoProvider = new KeyVaultCryptoProvider()
                }
            };

            var value = _jwtSecurityTokenService.CreateToken(_configuration.ClientId,
                $"{_configuration.BaseUrl}/token", claimsIdentity, signingCredentials);

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogLafError(LogEvents.OidcService, "CreateJwtAssertion: {Message}", ex.Message);
            throw;
        }
    }
    private static Dictionary<string, string> CreateRequestContentItems(OpenIdConnectMessage? openIdConnectMessage,
                                                                            string? jwtAssertion,
                                                                            bool includeCodeVerifierInRequest)
    {
        //Note: convert the hard coded strings to constants. Due to time constraints this will be done later.            
        var contentItems = new Dictionary<string, string>
                                {
                                    {"grant_type", "authorization_code" },
                                    {"code", openIdConnectMessage?.Code ?? string.Empty },
                                    {"redirect_uri", openIdConnectMessage?.RedirectUri ?? string.Empty },
                                    {"client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                                    {"client_assertion", jwtAssertion ?? string.Empty }
                                };

        if (includeCodeVerifierInRequest)
        { contentItems.Add("code_verifier", openIdConnectMessage?.GetParameter("code_verifier") ?? string.Empty); }

        return contentItems;
    }
}