namespace Ofgem.OneLogin.SharedLibrary.Services;

public interface IRedirectService
{
    Task<string> GetEnvironmentDomainAsync();
    Task<string> GetPostLogoutRedirectUriAsync();
}