using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.ExternalUsers
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class ExternalUserCompletedModel(
        ILogger<ExternalUserCompletedModel> logger)
        : PageModel
    {
        [BindProperty(SupportsGet = true)] public Ofgem.LAF.SharedLibrary.Models.User ExternalUser { get; set; } = new();

        [BindProperty] public string EmailAddress { get; set; } = string.Empty;

        [BindProperty] public bool IsSuccessfulAdd { get; set; }
        [BindProperty] public bool IsSuccessfulEdit { get; set; }
        [BindProperty] public bool IsSuccessfulDeactivate { get; set; }
        [BindProperty] public bool IsSuccessfulReactivate { get; set; }


        public async Task OnGetSuccessAdd()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("ExternalUserCompletedModel - OnGetSuccessAdd");
                IsSuccessfulAdd = true;
            });
        }
        public async Task OnGetSuccessEdit(string emailAddress)
        {
            EmailAddress = emailAddress;

            await Task.Run(() =>
            {
                logger.LogInformation("ExternalUserCompletedModel - OnGetSuccessEdit");
                IsSuccessfulEdit = true;
            });
        }
        public async Task OnGetSuccessReactivate()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("ExternalUserCompletedModel - OnGetSuccessReactivate");
                IsSuccessfulReactivate = true;
            });
        }
        public async Task OnGetSuccessDeactivation()
        {
            await Task.Run(() =>
            {
                logger.LogInformation("ExternalUserCompletedModel - OnGetSuccessDeactivation");
                IsSuccessfulDeactivate = true;
            });
        }

    }
}
