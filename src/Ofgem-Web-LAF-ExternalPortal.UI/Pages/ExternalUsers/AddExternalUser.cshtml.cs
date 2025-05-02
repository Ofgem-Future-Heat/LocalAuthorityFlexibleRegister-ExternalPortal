using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.ExternalUsers
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class AddExternalUserModel(
        ILogger<ExternalUserPage> logger,
        IHttpContextAccessor httpContextAccessor,
        IUserManagementServiceUI userService)
        : ExternalUserPage(userService)
    {

        public async Task OnGet()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("AddExternalUser - OnGet");

                PageDataInitialise();
            });
        }

        public async Task<IActionResult> OnPostSave()
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "AddExternalUser - OnPostSave - ");

            if (httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            PageDataInitialise();

            ValidFirstname();
            ValidLastname();
            ValidEmail();
            ValidLocalAuthority(out var baseLocalAuthority);
            if (await ValidUserTypeAsync()) CreateUserTypesList();

            if (Errors.Count > 0 || baseLocalAuthority is null) return Page();

            var (success, errorMessage) = await userService.CreateExternalUserAsync(ExternalUser, baseLocalAuthority);

            if (success)
            {
                return RedirectToPage(
                    LafPages.ExternalUserCompleted.ROUTE,
                    LafPages.ExternalUserCompleted.METHOD_GET_SUCCESS_ADD);
            }

            Errors.Add(("", errorMessage));

            return Page();
        }
    }
}
