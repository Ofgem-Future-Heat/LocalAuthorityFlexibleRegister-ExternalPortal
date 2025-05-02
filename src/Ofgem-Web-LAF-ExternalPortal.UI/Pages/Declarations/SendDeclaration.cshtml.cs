using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.LAF.SharedLibrary.Extensions;
using static Ofgem_Web_LAF_ExternalPortal.Models.SingleDeclarationJourney;
using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations;
[AutoValidateAntiforgeryToken]
[Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
[Authorize(Policy = ClaimsConstants.LinkedAccount)]
[Authorize(Policy = "MustHaveClaimLocalAuthorities")]
public class SendDeclaration(
    ILogger<SendDeclaration> logger,
    Services.ILaManagementService laManagementService,
    IHttpContextAccessor httpContextAccessor) : PageModel
{

    [BindProperty] public string? SelectedMethod { get; set; } = string.Empty;

    [BindProperty] public string UserLaDetails { get; set; } = string.Empty;

    [BindProperty] public bool IsSingleDeclarationAllowed { get; set; }

    public bool HasError { get; set; }
    public string DisplayMessage { get; set; } = string.Empty;

    public Models.Claims? Claims { get; set; }

    public async Task OnGet()
    {
        if (httpContextAccessor.HttpContext is null)
            throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext), @"HttpContext is null");

        Claims = new Models.Claims(httpContextAccessor.HttpContext.User);

        UserLaDetails = string.Join(", ", Claims.AsArrayOfStrings());

        IsSingleDeclarationAllowed = false;

        try
        {
            var statuses = new[]
            {
                SoiStatusV2.ToBeAssessed,
                SoiStatusV2.PassedAssessment,
                SoiStatusV2.AwaitingSignOff
            };

            var soiFilter = new Ofgem.LAF.SharedLibrary.Models.SoiFilter
            {
                BaseLocalAuthority = Claims.BaseLocalAuthority?.OnsCode ?? string.Empty,
                LAs = Claims.AsArrayOfStrings() ?? [],
                Status = statuses,
                PageIndex = 0,
                RecordsPerPage = 1000
            };

            var results = await laManagementService.GetPagedSoiListAsync(soiFilter)
                          ?? throw new ArgumentNullException($"Statement of intent data does not exist for {Claims.BaseLocalAuthority?.Name}");

            var validResults = (results.StatementOfIntents ?? new List<StatementOfIntent>())
                .Where(x =>
                    Enum.GetValues<ForSchemeEnum>().Contains(x.ForScheme) &&
                    x.ForScheme != ForSchemeEnum.NoScheme)
                .ToArray();

            // store the available local authorities
            var data = new Models.SingleDeclarationJourney();

            if (validResults.Any())
            {
                var userLaNames = Claims.LaList != null
                    ? Claims.LaList.Select(x => x.Name)
                    : [];

                var returnedLaNames = validResults
                    .Select(x => x.LocalAuthority)
                    .Distinct();

                IsSingleDeclarationAllowed = userLaNames
                    .Intersect(returnedLaNames)
                    .Any();

                foreach (var statementOfIntent in validResults)
                {
                    if (Claims.LaList != null && Claims.LaList.Any(x => x.Name == statementOfIntent.LocalAuthority))
                    {
                        foreach (var userLocalAuthority in Claims.LaList)
                        {
                            if (userLocalAuthority.Name != statementOfIntent.LocalAuthority) continue;

                            if (userLocalAuthority.IsBaseLocalAuthority != null && (bool)userLocalAuthority.IsBaseLocalAuthority)
                            {
                                data.BaseLocalAuthority = new AvailableLocalAuthority()
                                {
                                    OnsCode = userLocalAuthority.OnsCode ?? "",
                                    Name = userLocalAuthority.Name ?? "",
                                    IsBase = userLocalAuthority.IsBaseLocalAuthority ?? false,
                                    AvailableSoiList = new List<AvailableSoi>
                                    {
                                        new AvailableSoi
                                        {
                                            StatementOfIntentId = statementOfIntent.StatementOfIntentId,
                                            VersionNumber = statementOfIntent.VersionNumber ?? "",
                                            SoiStatus = statementOfIntent.SoiStatus,
                                            DateAdded = statementOfIntent.DateAdded ?? null,
                                            ForScheme = statementOfIntent.ForScheme
                                        }
                                     }
                                };

                                continue;
                            }

                            var source = data.LocalAuthorityList.Find(la => la.Name == userLocalAuthority.Name);

                            if (source == null)
                            {
                                data.LocalAuthorityList.Add(new AvailableLocalAuthority()
                                {
                                    OnsCode = userLocalAuthority.OnsCode ?? "",
                                    Name = userLocalAuthority.Name ?? "",
                                    IsBase = userLocalAuthority.IsBaseLocalAuthority ?? false,
                                    AvailableSoiList = new List<AvailableSoi>
                                    {
                                        new AvailableSoi
                                        {
                                            StatementOfIntentId = statementOfIntent.StatementOfIntentId,
                                            VersionNumber = statementOfIntent.VersionNumber ?? "",
                                            SoiStatus = statementOfIntent.SoiStatus,
                                            DateAdded = statementOfIntent.DateAdded ?? null,
                                            ForScheme = statementOfIntent.ForScheme
                                        }
                                    }
                                });
                            }
                            else
                            {
                                source.AvailableSoiList.Add(new AvailableSoi
                                {
                                    VersionNumber = statementOfIntent.VersionNumber ?? "",
                                    SoiStatus = statementOfIntent.SoiStatus,
                                    DateAdded = statementOfIntent.DateAdded ?? null,
                                    ForScheme = statementOfIntent.ForScheme
                                });
                            }
                        }
                    }
                }

                data.LocalAuthorityList = data.LocalAuthorityList.OrderBy(la => la.Name).ToList();
            }

            TempData.Put(TempDataKeys.SingleDeclarationData, data);
        }
        catch (Exception ex)
        {
            logger.LogLafError(LogEvents.GetSoi, $"An issue occurred retrieving soi data, {ex.Message}");
            DisplayMessage = "An issue occurred retrieving data";
        }
    }


    public Task<IActionResult> OnPost()
    {
        switch (SelectedMethod)
        {
            case Constants.DeclarationCreationUploadText:

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.UploadTemplate.ROUTE));

            case Constants.DeclarationCreationSingleText:

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.SelectLocalAuthority.ROUTE));

            default:
                DisplayMessage = "Select an option to send declarations to Ofgem";
                HasError = true;
                break;
        }

        return Task.FromResult<IActionResult>(Page());
    }
}