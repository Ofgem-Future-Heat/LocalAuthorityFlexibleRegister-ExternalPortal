namespace Ofgem.OneLogin.SharedLibrary.Services;

public interface IAzureIdentityService
{
    Task<string> AuthenticationCallback(string authority, string resource, string scope);
}