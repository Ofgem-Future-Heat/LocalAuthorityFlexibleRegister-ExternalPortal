using Ofgem.LAF.SharedLibrary.Models;

namespace Ofgem.OneLogin.SharedLibrary.UserManagement.Services;

public interface ILocalAuthorityManagementService
{
    Task<List<LocalAuthority>?> GetAssociatedLocalAuthoritiesAsync(User externalUser);
}