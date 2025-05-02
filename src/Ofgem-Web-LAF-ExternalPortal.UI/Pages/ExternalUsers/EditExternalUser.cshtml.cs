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
    public class EditExternalUserModel(
        ILogger<ExternalUserPage> logger,
        IHttpContextAccessor httpContextAccessor,
        IUserManagementServiceUI userService)
        : ExternalUserPage(userService)
    {

        public async Task OnGetById(string userId)
        {
            await Task.Run(() =>
            {
                logger.LogLafInformation(LogEvents.ExternalUsers, $"EditExternalUser - OnGetById - userId:{userId}");

                GetExternalUser(userId);

                PageDataInitialise();
            });
        }

        public async Task<IActionResult> OnPostUpdate()
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "EditExternalUser - OnPostUpdate - ");

            if (httpContextAccessor.HttpContext == null) throw new ArgumentException("Unable to determine the user");

            PageDataInitialise();

            ValidFirstname();
            ValidLastname();
            ValidEmail();
            ValidLocalAuthority(out var baseLocalAuthority);
            if (await ValidUserTypeAsync()) CreateUserTypesList();

            if (Errors.Count > 0 || baseLocalAuthority is null) return Page();

            var (success, errorMessage) = await userService.UpdateExternalUserAsync(ExternalUser, baseLocalAuthority);

            if (success)
            {
                return RedirectToPage(
                    LafPages.ExternalUserCompleted.ROUTE,
                    LafPages.ExternalUserCompleted.METHOD_GET_SUCCESS_EDIT,
                    new
                    {
                        emailAddress = ExternalUser.EmailAddress
                    });
            }

            Errors.Add(("", errorMessage));

            return Page();
        }

        private void GetExternalUser(string userId)
        {
            logger.LogLafInformation(LogEvents.ExternalUsers, "EditExternalUser - GetExternalUser");

            var abc = userService.GetExternalUserAsync(userId);

            if (abc.Result is null)
            {
                Errors.Add(("", "An issue occurred retrieving the external user data"));
                return;
            }

            ExternalUser = abc.Result;

            if (ExternalUser.ExternalUserLocalAuthorities is not { Count: > 0 }) return;

            var source =
                ExternalUser.ExternalUserLocalAuthorities.First(f => f.IsBaseLocalAuthority != null && (bool)f.IsBaseLocalAuthority);

            HomeBaseLocalAuthority = $"{source.OnsCode} {source.Name}";

            if (source.OnsCode != null) SelectedLa = source.OnsCode;

        }
    }
}
