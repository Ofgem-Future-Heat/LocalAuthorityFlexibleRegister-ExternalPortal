using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Models;
using Ofgem.LAF.SharedLibrary.Enums;
using static Ofgem_Web_LAF_ExternalPortal.Models.SingleDeclarationJourney;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Single
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class SelectStatementOfIntentModel(
        ILogger<SelectStatementOfIntentModel> logger) : PageModel
    {

        [BindProperty] public string? SelectedLa { get; set; } = string.Empty;

        [BindProperty] public Guid? SelectedSoi { get; set; }

        public SingleDeclarationJourney? SingleDeclarationJourney { get; set; }

        public List<AvailableSoi>? AvailableSoiList { get; set; }

        public bool HasError => !string.IsNullOrWhiteSpace(DisplayMessage);

        public string DisplayMessage { get; set; } = string.Empty;

        public string? ErrorId { get; set; }

        public void OnGet()
        {
            logger.LogInformation("SelectStatementOfIntentModel - OnGet");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                      ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                SelectedSoi = SingleDeclarationJourney.StatementOfIntentId;

                GetAvailableSoi(SingleDeclarationJourney);

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SelectStatementOfIntentModel - OnGet ");
                DisplayMessage = "An issue occurred retrieving data";
            }
        }

        public Task<IActionResult> OnPost()
        {
            logger.LogInformation("SelectStatementOfIntentModel - OnPost");

            try
            {
                SingleDeclarationJourney
                    = TempData.Get<SingleDeclarationJourney>(TempDataKeys.SingleDeclarationData)
                      ?? throw new ArgumentNullException(nameof(SingleDeclarationJourney), @"Single declaration data does not exist");

                GetAvailableSoi(SingleDeclarationJourney);

                if (SelectedSoi == null)
                {
                    ErrorId = "SelectSoi";
                    DisplayMessage = "Select a Statement of Intent version";
                    return Task.FromResult<IActionResult>(Page());
                }

                if (AvailableSoiList is null)
                    throw new ArgumentNullException($"Failed to get the statment of intent data");

                var source = AvailableSoiList.Find(s => s.StatementOfIntentId == SelectedSoi);

                if (source == null)
                {
                    ErrorId = "SelectSoi";
                    DisplayMessage = "Select a Statement of Intent version";

                    GetAvailableSoi(SingleDeclarationJourney);

                    return Task.FromResult<IActionResult>(Page());
                }

                var soi = AvailableSoiList.Find(x => x.StatementOfIntentId == SelectedSoi);

                if (soi is null) throw new ArgumentException($@"Unable to find selected StatementOfIntent {SelectedSoi}", nameof(soi));

                SingleDeclarationJourney.StatementOfIntentId = SelectedSoi;
                SingleDeclarationJourney.ForScheme = soi.ForScheme;
                SingleDeclarationJourney.DateAdded = soi.DateAdded;


                if (soi.DateAdded is null) throw new ArgumentException($@"Selected StatementOfIntent has a null DateAdded property{SelectedSoi}", nameof(soi));

                var soiSchemeDescription = soi.ForScheme switch
                {
                    ForSchemeEnum.ECO4 => StatementOfIntentPublishedForDescriptions.StatementOfIntentForEco4Flex,
                    ForSchemeEnum.ECO4andGBIS => StatementOfIntentPublishedForDescriptions.StatementOfIntentForEco4AndGbiSchemeFlex,
                    _ => "No scheme"
                };

                SingleDeclarationJourney.StatementOfIntentDetails = $"{source.VersionNumber} - submitted on {(DateTime)soi.DateAdded:dd/MM/yyyy} - scheme {soiSchemeDescription}";

                TempData.Put(TempDataKeys.SingleDeclarationData, SingleDeclarationJourney);

                return Task.FromResult<IActionResult>(RedirectToPage(LafPages.UniqueReferenceNumber.ROUTE));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SelectStatementOfIntentModel - OnPost ");
                DisplayMessage = "An issue occurred posting data";
            }

            return Task.FromResult<IActionResult>(Page());
        }

        private void GetAvailableSoi(SingleDeclarationJourney singleDeclarationJourney)
        {
            SelectedLa = singleDeclarationJourney.OnsCode;

            if (SelectedLa == singleDeclarationJourney.BaseLocalAuthority?.OnsCode)
            {
                AvailableSoiList = singleDeclarationJourney.BaseLocalAuthority.AvailableSoiList;
            }
            else
            {
                var source = singleDeclarationJourney.LocalAuthorityList.Find(la => la.OnsCode == SelectedLa);

                if (source != null)
                {
                    AvailableSoiList = source.AvailableSoiList;
                }
            }
        }
    }
}
