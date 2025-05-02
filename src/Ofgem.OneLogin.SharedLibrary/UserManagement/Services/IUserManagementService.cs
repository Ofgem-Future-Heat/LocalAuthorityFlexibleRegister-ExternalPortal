using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem.OneLogin.SharedLibrary.UserManagement.Services;

public interface IUserManagementService
{
    Task<User?> SyncExternalUserAsync(string providerId, string emailAddress);
    Task<User?> SyncExternalUserAsync(TokenValidatedContext tokenValidatedContext);
}