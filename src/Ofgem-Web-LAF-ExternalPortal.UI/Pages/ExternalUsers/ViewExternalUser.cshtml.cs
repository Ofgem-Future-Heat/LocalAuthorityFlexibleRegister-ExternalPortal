using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Services;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.ExternalUsers
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class ViewExternalUserModel(
        ILogger<ViewExternalUserModel> logger,
        IUserManagementServiceUI userManagementServiceUi)
        : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Ofgem.LAF.SharedLibrary.Models.User ExternalUser { get; set; } = new()
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            EmailAddress = string.Empty,
            UserType = ExternalUserType.LocalAuthorityOfficer
        };

        [BindProperty] public required string HomeBaseLocalAuthority { get; set; } = string.Empty;


        [BindProperty] public string Message { get; set; } = string.Empty;
        [BindProperty] public bool HasMessage => Message.Length > 0;


        public async Task OnGet(string userId)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "ViewExternalUser - OnGet");

            try
            {
                var user = await userManagementServiceUi.GetExternalUserAsync(userId);

                if (user == null)
                {
                    Message = ErrorMessagesExternalSite.ViewExternalUser.AnIssueOccurredRetrievingTheExternalUserData;
                    return;
                }

                ExternalUser = user;

                if (ExternalUser.ExternalUserLocalAuthorities is not { Count: > 0 }) return;

                var source =
                    ExternalUser.ExternalUserLocalAuthorities.First(f => f.IsBaseLocalAuthority != null && (bool)f.IsBaseLocalAuthority);

                HomeBaseLocalAuthority = $"{source.OnsCode} {source.Name}";

            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.ExternalUsers, "ExternalUsers - GetExternalUser, userId {userId}, {Message}", userId, ex.Message);
                Message = ErrorMessagesExternalSite.SharedMessages.AnIssueOccurredRetrievingData;
            }
        }
    }
}
