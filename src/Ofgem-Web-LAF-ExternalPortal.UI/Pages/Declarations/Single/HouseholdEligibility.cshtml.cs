using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Single;

[AutoValidateAntiforgeryToken]
[Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
[Authorize(Policy = ClaimsConstants.LinkedAccount)]
[Authorize(Policy = "MustHaveClaimLocalAuthorities")]
public class HouseholdEligibility(
    ILogger<HouseholdEligibility> logger) : PageModel
{
    public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

    [BindProperty] public ThreePartDateNullable DateOfHouseholderEligibilityControl { get; set; } = new();

    public bool HasError => DisplayMessages.Count > 0;

    public List<string> DisplayMessages { get; set; } = [];
        
    public string? ErrorId { get; set; }

    public void OnGet()
    {
        logger.LogInformation("HouseholdEligibility - OnGet");

        try
        {
            SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                  ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

            DateOfHouseholderEligibilityControl
                = new ThreePartDateNullable(
                    source: SingleDeclarationJourney.DateOfHouseholderEligibility is null ? null : DateTime.ParseExact(SingleDeclarationJourney.DateOfHouseholderEligibility,
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture),
                    title:string.Empty,
                    titleToBeUsedInErrorMessage: "Date of Householder Eligibility",
                    firstHint: "",
                    secondHint: string.Empty,
                    showTheHighlightBar: true
                );

            TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HouseholdEligibility - OnGet");
            DisplayMessages.Add("An issue occurred retrieving data");
        }
    }

    public Task<IActionResult> OnPost()
    {
        logger.LogInformation("HouseholdEligibility - OnPost");

        try
        {
            SingleDeclarationJourney
                = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData) ??
                  throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

            if (DateOfHouseholderEligibilityControl.HasErrors())
            {
                DisplayMessages.Add(DateOfHouseholderEligibilityControl.ErrorMessage);
                return Task.FromResult<IActionResult>(Page());
            }

            var dateOfHouseholderEligibility =
                new DateTime(
                    DateOfHouseholderEligibilityControl.Year!.Value,
                    DateOfHouseholderEligibilityControl.Month!.Value,
                    DateOfHouseholderEligibilityControl.Day!.Value, 0, 0, 0, DateTimeKind.Utc);

            if (IsFutureDate())
            {
                ErrorId = "PassportIssuedDate";
                DisplayMessages.Add("The date for 'Date of Householder Eligibility' cannot be in the future.");
            }

            if (IsBeforeStatementOfIntentDateAdded())
            {
                ErrorId = "PassportIssuedDate";
                DisplayMessages.Add($"Date of statement of intent published ({SingleDeclarationJourney.DateAdded:dd/MM/yyyy}) must be OLDER THAN the Date of household eligibility");
            }

            if (HasError) return Task.FromResult<IActionResult>(Page());

            SingleDeclarationJourney.DateOfHouseholderEligibility = dateOfHouseholderEligibility.ToString("dd/MM/yyyy");
            TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

            return Task.FromResult<IActionResult>(RedirectToPage(LafPages.WasLocalAuthorityConsulted.ROUTE));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HouseholdEligibility - OnPost");
            DisplayMessages.Add("An issue occurred posting data");
        }

        return Task.FromResult<IActionResult>(Page());
    }

    private bool IsFutureDate()
        => !DateOfHouseholderEligibilityControl.HasError &&
           new DateTime(DateOfHouseholderEligibilityControl.Year!.Value,
               DateOfHouseholderEligibilityControl.Month!.Value,
               DateOfHouseholderEligibilityControl.Day!.Value, 0, 0, 0, DateTimeKind.Utc).Date > DateTime.Today;

    private bool IsBeforeStatementOfIntentDateAdded()
        => !DateOfHouseholderEligibilityControl.HasError &&
           SingleDeclarationJourney is { DateAdded: not null } && 
           new DateTime(DateOfHouseholderEligibilityControl.Year!.Value,
               DateOfHouseholderEligibilityControl.Month!.Value,
               DateOfHouseholderEligibilityControl.Day!.Value, 0, 0, 0, DateTimeKind.Utc).Date < SingleDeclarationJourney.DateAdded.Value.Date;
}