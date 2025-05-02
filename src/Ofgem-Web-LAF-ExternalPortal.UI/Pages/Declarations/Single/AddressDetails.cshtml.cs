using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Single
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class AddressDetails(
        ILogger<AddressDetails> logger) : PageModel
    {

        [BindProperty] public string? Line1 { get; set; } = string.Empty;
        [BindProperty] public string? Line2 { get; set; } = string.Empty;
        [BindProperty] public string? Postcode { get; set; } = string.Empty;


        [BindProperty] public bool Line1Error { get; set; }
        [BindProperty] public bool Line2Error { get; set; }
        [BindProperty] public bool PostcodeError { get; set; }


        [BindProperty] public string? Line1ErrorMessage { get; set; } = string.Empty;
        [BindProperty] public string? Line2ErrorMessage { get; set; } = string.Empty;
        [BindProperty] public string? PostcodeErrorMessage { get; set; } = string.Empty;


        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        public bool HasError => DisplayMessages.Count > 0;

        public List<(string Id, string Message)> DisplayMessages { get; set; } = [];


        public void OnGet()
        {
            logger.LogInformation("AddressDetails - OnGet");
            try
            {
                SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                  ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                Line1 = SingleDeclarationJourney.Address1;
                Line2 = SingleDeclarationJourney.Address2;
                Postcode = SingleDeclarationJourney.Postcode;

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddressDetails - OnGet");
                DisplayMessages.Add(("", "An issue occurred retrieving data"));
            }
        }

        public Task<IActionResult> OnPost()
        {
            try
            {
                logger.LogInformation("AddressDetails - OnPost");

                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                      ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                ADDRSS_001_Declaration_Has_Address_Line_1(Line1);
                ADDRSS_002_Declaration_Has_Address_Line_2(Line2);
                ADDRSS_003_Declaration_PostCode(Postcode);

                if (DisplayMessages.Count > 0)
                {
                    TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                    return Task.FromResult<IActionResult>(Page());
                }

                SingleDeclarationJourney.Address1 = Line1 ?? "";
                SingleDeclarationJourney.Address2 = Line2 ?? "";
                SingleDeclarationJourney.Postcode = Postcode ?? "";

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.HouseholdEligibility.ROUTE));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddressDetails - OnPost");
                DisplayMessages.Add(("", "An issue occurred posting data"));
            }

            return Task.FromResult<IActionResult>(Page());
        }

        private void ADDRSS_001_Declaration_Has_Address_Line_1(string? line1)
        {
            if (string.IsNullOrEmpty(line1) || string.Equals(line1, Constants.NotApplicable, StringComparison.OrdinalIgnoreCase))
            {
                DisplayMessages.Add(("Line1", RuleErrorMessages.ADDRSS_001_Has_Address_Line_1));
                Line1Error = true;
                Line1ErrorMessage = RuleErrorMessages.ADDRSS_001_Has_Address_Line_1;
            }
        }

        private void ADDRSS_002_Declaration_Has_Address_Line_2(string? line2)
        {
            if (string.IsNullOrEmpty(line2) || string.Equals(line2, Constants.NotApplicable, StringComparison.OrdinalIgnoreCase))
            {
                DisplayMessages.Add(("Line2", RuleErrorMessages.ADDRSS_002_Has_Address_Line_2));
                Line2Error = true;
                Line2ErrorMessage = RuleErrorMessages.ADDRSS_002_Has_Address_Line_2;
            }
        }

        private void ADDRSS_003_Declaration_PostCode(string? postcode)
        {
            // ADDRSS_003_Declaration_Has_PostCode
            if (string.IsNullOrEmpty(postcode) || string.Equals(postcode, Constants.NotApplicable, StringComparison.OrdinalIgnoreCase))
            {
                DisplayMessages.Add(("Postcode", RuleErrorMessages.ADDRSS_003_Has_PostCode));
                PostcodeError = true;
                PostcodeErrorMessage = RuleErrorMessages.ADDRSS_003_Has_PostCode;
                return;
            }


            // ADDRSS_004_Postcode_Has_Correct_Format
            var result = Regex.IsMatch(
                postcode,
                "^(([A-Z][0-9]{1,2})|(([A-Z][A-HJ-Y][0-9]{1,2})|(([A-Z][0-9][A-Z])|([A-Z][A-HJ-Y][0-9]?[A-Z])))) [0-9][A-Z]{2}$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(500)
            );

            if (result) return;

            DisplayMessages.Add(("Postcode", RuleErrorMessages.ADDRSS_004_Postode_Has_Incorrect_Format));
            PostcodeError = true;
            PostcodeErrorMessage = RuleErrorMessages.ADDRSS_004_Postode_Has_Incorrect_Format;
        }
    }
}
