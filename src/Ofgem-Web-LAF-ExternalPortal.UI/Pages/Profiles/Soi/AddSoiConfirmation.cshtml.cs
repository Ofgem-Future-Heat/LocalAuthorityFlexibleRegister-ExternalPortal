using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Profiles.Soi
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class AddSoiConfirmation(ILogger<AddSoiConfirmation> logger) : SoiPage
    {

        [BindProperty] public Guid StatementOfIntentId { get; set; }

        public void OnGet(Guid statementOfIntentId)
        {
            logger.LogInformation("AddSoiConfirmation - OnSoiSuccessAdd");

            StatementOfIntentId = statementOfIntentId;
            TempData.Remove(SoiDataKey);
        }
    }
}
