using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Ofgem.API.LAF.LocalAuthorityManagement.Application;
using Ofgem.LAF.SharedLibrary.Extensions;

namespace Ofgem.OneLogin.SharedLibrary.Services;

internal class AzureIdentityService(ILogger<AzureIdentityService> logger) : IAzureIdentityService
{
    private readonly ILogger<IAzureIdentityService> _logger = logger;
    private readonly string[] _azureVaultScope = ["https://vault.azure.net/.default"];

    public async Task<string> AuthenticationCallback(string authority, string resource, string scope)
    {
        try
        {
            var chainedTokenCredential = new ChainedTokenCredential(
                new ManagedIdentityCredential(),
                new AzureCliCredential());

            var token = await chainedTokenCredential.GetTokenAsync(
                new TokenRequestContext(scopes: _azureVaultScope));

            return token.Token;
        }
        catch (Exception ex)
        {
            _logger.LogLafError(LogEvents.AuthenticationCallback, "{Message}", ex.Message);
            throw;
        }
    }
}