using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Ofgem.OneLogin.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;

namespace Ofgem.OneLogin.SharedLibrary.Services;

public class RedirectService : IRedirectService
{
    private readonly IConfiguration _configuration;

    public RedirectService(IConfiguration? configuration, IHttpContextAccessor? httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = configuration;
    }

    public async Task<string> GetEnvironmentDomainAsync()
    {
        var task = Task.Run(() => CheckEnvironmentUrl(_configuration[EnvironmentConstants.EnvironmentConfig.EnvUrlKey]));

        return await task;
    }

    private static string CheckEnvironmentUrl(string? environmentUrl)
    {
        ArgumentNullException.ThrowIfNull(environmentUrl);
        return environmentUrl;
    }

    /// <summary>
    /// Post signing out redirect Uri
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetPostLogoutRedirectUriAsync()
    {
        return $"{await GetEnvironmentDomainAsync()}{OneLoginConstants.SignedOutUrl}";
    }
}