using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Extensions;
using Microsoft.AspNetCore.Authorization;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.ExternalUsers
{

    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class DeactivateExternalUserModel(
        ILogger<DeactivateExternalUserModel> logger,
        Services.IUserManagementServiceUI userService)
        : PageModel
    {
        [BindProperty] public string UserId { get; set; } = string.Empty;

        [BindProperty] public string FirstName { get; set; } = string.Empty;

        [BindProperty] public string LastName { get; set; } = string.Empty;

        [BindProperty] public string EmailAddress { get; set; } = string.Empty;

        [BindProperty] public ExternalUserType UserType { get; set; }

        [BindProperty] public string HomeBaseLocalAuthority { get; set; } = string.Empty;

        [BindProperty] public bool ShowNotification { get; set; }

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;


        public void OnGetById(string userId, string firstName, string lastName, string emailAddress, ExternalUserType userType, string homeBaseLocalAuthority)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            UserType = userType;
            HomeBaseLocalAuthority = homeBaseLocalAuthority;

            logger.LogLafInformation(LogEvents.ExternalUsers, $"DeactivateExternalUserModel - OnGet - userId: {UserId}, firstName: {FirstName}, lastName:{LastName}");

        }

        public async Task<IActionResult> OnPostDeactivate(string userId)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, $"DeactivateExternalUserModel - OnPostDeactivate - userId: {userId}, firstName: {FirstName}, lastName:{LastName}");
            
            try
            {
                var (success, errorMessage) = await userService.DeactivateExternalUserAsync(userId);

                if (success)
                {
                    return RedirectToPage(
                        LafPages.ExternalUserCompleted.ROUTE,
                        LafPages.ExternalUserCompleted.METHOD_GET_SUCCESS_DEACTIVATION);
                }

                DisplayMessage = errorMessage;
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.ExternalUsers, "DeactivateExternalUserModel - OnPostDeactivate - {Message}", ex.Message);
                DisplayMessage = ErrorMessagesExternalSite.DeactivateExternalUser.AnIssueOccurredDeactivatingTheUser;
            }

            return RedirectToPage(LafPages.ExternalUsers.ROUTE);
        }
    }
}