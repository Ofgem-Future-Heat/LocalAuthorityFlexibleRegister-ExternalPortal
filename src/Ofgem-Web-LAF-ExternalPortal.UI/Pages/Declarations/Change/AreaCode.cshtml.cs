using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using System.Text.RegularExpressions;
using Ofgem.LAF.SharedLibrary.Models;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem_Web_LAF_ExternalPortal.Exceptions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Change
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public partial class AreaCodeModel(
        ILogger<AreaCodeModel> logger,
        Services.IDeclarationManagementService declarationManagementService,
        Services.ILaManagementService laManagementService)
        : PageModel
    {

        [BindProperty]
        public string? Urn { get; set; } = string.Empty;

        [BindProperty]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public string? SelectedLa { get; set; } = string.Empty;

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; }


        [BindProperty] public string AllowedLaDetails { get; set; } = string.Empty;

        private Models.Declaration? Declaration { get; set; }


        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("DeclarationDetailModel - OnGetWithId");

            GetUsersAllowedLocalAuthorities();

            await RefreshData(declarationId, null);

            if (Declaration is null)
            {
                HasError = true;
                return;
            }

            DeclarationId = Declaration.DeclarationId;
            Urn = Declaration.Urn;
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("DeclarationDetailModel - OnPost");


            if (string.IsNullOrEmpty(SelectedLa))
            {
                DisplayMessage = ErrorMessagesExternalSite.AreaCode.AreaCodeIsAMandatoryField;
                HasError = true;
                return Page();
            }

            if (SelectedLa.Length != 9)
            {
                DisplayMessage = ErrorMessagesExternalSite.AreaCode.AreaCodeShouldBeInTheFormatA12345678;
                HasError = true;
                return Page();
            }

            if (!RegexAlphabetic().IsMatch(SelectedLa.AsSpan(0, 1)))
            {
                DisplayMessage = ErrorMessagesExternalSite.AreaCode.AreaCodeShouldBeInTheFormatA12345678;
                HasError = true;
                return Page();
            }

            if (!RegexNumbers().IsMatch(SelectedLa.AsSpan(1, 8)))
            {
                DisplayMessage = ErrorMessagesExternalSite.AreaCode.AreaCodeShouldBeInTheFormatA12345678;
                HasError = true;
                return Page();
            }

            // validate the code is allowed


            if (!AllowedLaDetails.Contains(SelectedLa))
            {
                DisplayMessage = ErrorMessagesExternalSite.AreaCode.AreaCodeShouldBeOneOfTheLocalAuthoritiesInTheList + AllowedLaDetails;
                HasError = true;
                return Page();
            }


            await RefreshData(DeclarationId, SelectedLa);

            if (Declaration is null)
            {
                HasError = true;
                return Page();
            }

            Declaration.LAAreaCode = SelectedLa;

            declarationManagementService.UpdateDeclarationInCache(Declaration);

            return RedirectToPage(
                Extensions.LafPages.DeclarationDetail.ROUTE,
                Extensions.LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                new
                {
                    declarationId = DeclarationId
                });
        }



        private async Task RefreshData(Guid declarationId, string? preSelectedLa)
        {
            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                SelectedLa = Declaration.LAAreaCode;

                if (preSelectedLa is not null)
                {
                    SelectedLa = preSelectedLa;
                }

                var result = await laManagementService.GetLocalAuthoritiesAsync();

                if (result is null)
                {
                    logger.LogLafError(LogEvents.GetLocalAuthorities, "No local authority data returned");
                }
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.GetDeclaration, $"An issue occurred retrieving declaration data, {ex.Message}");
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.AnIssueOccurredRetrievingData;
            }
        }

        private void GetUsersAllowedLocalAuthorities()
        {
            if (User is null) return;

            var claimsIdentity = User.Identity as ClaimsIdentity;

            if (claimsIdentity is null) return;

            var las = claimsIdentity.FindFirst("UserLocalAuthorities");

            if (las is null) return;

            var laList = JsonSerializer.Deserialize<List<ExternalUserLocalAuthority>>(las.Value);

            if (laList is null) return;

            AllowedLaDetails = laList.Aggregate(string.Empty, (current, localAuthority) => current + $"{localAuthority.OnsCode} - {localAuthority.Name}, ");
        }



        [GeneratedRegex(@"^[0-9]+$")]
        private static partial Regex RegexNumbers();

        [GeneratedRegex(@"^[A-Z]+$")]
        private static partial Regex RegexAlphabetic();
    }
}
