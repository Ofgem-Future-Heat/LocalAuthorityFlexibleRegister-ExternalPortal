using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.LAF.SharedLibrary.Extensions;
using Microsoft.AspNetCore.Authorization;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.ExternalUsers
{

    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class ReactivateExternalUserModel(
        ILogger<ReactivateExternalUserModel> logger,
        Services.IUserManagementServiceUI userService)
        : PageModel
    {
        [BindProperty] public string UserId { get; set; } = string.Empty;

        [BindProperty] public string FirstName { get; set; } = string.Empty;

        [BindProperty] public string LastName { get; set; } = string.Empty;

        [BindProperty] public string EmailAddress { get; set; } = string.Empty;

        [BindProperty] public string UserType { get; set; } = string.Empty;

        [BindProperty] public string HomeBaseLocalAuthority { get; set; } = string.Empty;


        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasDisplayMessage => DisplayMessage.Length > 0;


        public void OnGetById(string userId, string firstName, string lastName, string emailAddress, string userType, string homeBaseLocalAuthority)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            UserType = userType;
            HomeBaseLocalAuthority = homeBaseLocalAuthority;

            logger.LogLafInformation(LogEvents.ExternalUsers, $"ReactivateExternalUserModel - OnGet - userId: {UserId}, firstName: {FirstName}, lastName:{LastName}");

        }

        public async Task<IActionResult> OnPostReactivate(string userId)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, $"ReactivateExternalUserModel - OnPostReactivate - userId: {userId}, firstName: {FirstName}, lastName:{LastName}");
            
            try
            {
                var (success, errorMessage) = await userService.ReactivateExternalUserAsync(userId);

                if (success)
                {
                    return RedirectToPage(
                        LafPages.ExternalUserCompleted.ROUTE,
                        LafPages.ExternalUserCompleted.METHOD_GET_SUCCESS_REACTIVATE);
                }

                DisplayMessage = errorMessage;
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.ExternalUsers, "ReactivateExternalUserModel - OnPostReactivate - {Message}", ex.Message);
                DisplayMessage = ErrorMessagesExternalSite.ReactivateExternalUser.AnIssueOccurredReactivatingTheUser;
            }

            return RedirectToPage(LafPages.ExternalUsers.ROUTE);
        }
    }
}