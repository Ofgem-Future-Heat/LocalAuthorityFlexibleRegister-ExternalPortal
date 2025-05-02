using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.ServiceBus;
using Ofgem.LAF.SharedLibrary.Constants;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Change
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class AddressOfHouseholdModel(
        ILogger<AddressOfHouseholdModel> logger,
        Services.IDeclarationManagementService declarationManagementService) : PageModel
    {
        [BindProperty] public string? Urn { get; set; } = string.Empty;

        [BindProperty] public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public string? AddressLine1 { get; set; } = string.Empty;

        [BindProperty] public string? AddressLine2 { get; set; } = string.Empty;

        [BindProperty] public string? Postcode { get; set; } = string.Empty;

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError => DisplayMessage.Length > 0;

        [BindProperty] public bool ErrorAddressLine1 { get; set; }

        [BindProperty] public bool ErrorAddressLine2 { get; set; }

        [BindProperty] public bool ErrorPostcode { get; set; }

        public string? ErrorId { get; set; }

        private Models.Declaration? Declaration { get; set; }

        public const string AddressLine1Id = "AddressLine1";

        public const string AddressLine2Id = "AddressLine2";

        public const string PostcodeId = "Postcode";

        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("AddressOfHouseholdModel - OnGetWithId");

            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                DeclarationId = Declaration.DeclarationId;
                Urn = Declaration.Urn;

                AddressLine1 = Declaration.AddressLine1;
                AddressLine2 = Declaration.AddressLine2;
                Postcode = Declaration.PostCode;

            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.GetDeclaration,
                    $"An issue occurred retrieving declaration data, {ex.Message}");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("AddressOfHouseholdModel - OnPost");

            if (!AddressValidation.IsAddressLineValid(AddressLine1))
            {
                DisplayMessage = RuleErrorMessages.ADDRSS_001_Has_Address_Line_1;
                ErrorId = AddressLine1Id;
                ErrorAddressLine1 = true;
                return Page();
            }

            if (!AddressValidation.IsAddressLineValid(AddressLine2))
            {
                DisplayMessage = RuleErrorMessages.ADDRSS_002_Has_Address_Line_2;
                ErrorId = AddressLine2Id;
                ErrorAddressLine2 = true;
                return Page();
            }

            if (!AddressValidation.IsPostcodeValid(Postcode))
            {
                DisplayMessage = RuleErrorMessages.ADDRSS_003_Has_PostCode;
                ErrorId = PostcodeId;
                ErrorPostcode = true;
                return Page();
            }

            if (!AddressValidation.IsPostcodeCorrect(Postcode))
            {
                DisplayMessage = RuleErrorMessages.ADDRSS_004_Postode_Has_Incorrect_Format;
                ErrorId = PostcodeId;
                ErrorPostcode = true;
                return Page();
            }

            Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(DeclarationId);

            if (Declaration is null)
            {
                DisplayMessage = "Unable to find the declaration.";
                return Page();
            }

            Declaration.AddressLine1 = AddressLine1;
            Declaration.AddressLine2 = AddressLine2;
            Declaration.PostCode = Postcode;

            declarationManagementService.UpdateDeclarationInCache(Declaration);

            return RedirectToPage(
                LafPages.DeclarationDetail.ROUTE,
                LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                new
                {
                    declarationId = DeclarationId
                });
        }
    }
}
