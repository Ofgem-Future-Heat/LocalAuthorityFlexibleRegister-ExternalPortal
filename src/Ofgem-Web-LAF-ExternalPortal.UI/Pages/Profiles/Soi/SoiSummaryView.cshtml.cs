using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem_Web_LAF_ExternalPortal.Models;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem.LAF.SharedLibrary.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Profiles.Soi
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]

    public class SoiSummaryViewModel(
        ILogger<SoiSummaryViewModel> logger,
        Services.ILaManagementService laManagementService) : PageModel
    {
        [BindProperty] public StatementOfIntent? StatementOfIntentModel { get; set; }

        [BindProperty] public bool HasError { get; set; }

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        public async Task OnGetById(Guid statementOfIntentId)
        {
            logger.LogInformation("SoiSummaryViewModel - OnGetById");

            await RefreshData(statementOfIntentId);
        }

        private async Task RefreshData(Guid statementOfIntentId)
        {
            try
            {
                StatementOfIntentModel = await laManagementService.GetSoiById(statementOfIntentId)
                                         ?? throw new ArgumentNullException($"{statementOfIntentId} does not exist");
            }
            catch (Exception ex)
            {
                DisplayMessage = $"An issue occurred retrieving SOI data, {ex.Message}";
                logger.LogLafError(LogEvents.GetSoi, DisplayMessage);
            }
        }
    }
}
