using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Change
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class DateOfStatementOfIntentPublicationModel(ILogger<DateOfStatementOfIntentPublicationModel> logger,
        Services.IDeclarationManagementService declarationManagementService) : PageModel
    {
        [BindProperty]
        public string? Urn { get; set; } = string.Empty;

        [BindProperty]
        public Guid DeclarationId { get; set; } = Guid.Empty;

        [BindProperty] public Models.ThreePartDate DateOfStatementOfIntentPublicationControl { get; set; } = new();

        [BindProperty] public string DisplayMessage { get; set; } = string.Empty;

        [BindProperty] public bool HasError { get; set; }

        private Models.Declaration? Declaration { get; set; }

        public async Task OnGetWithId(Guid declarationId)
        {
            logger.LogInformation("DateOfStatementOfIntentPublicationModel - OnGetWithId");

            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(declarationId)
                              ?? throw new ArgumentNullException($"{declarationId} does not exist");

                DeclarationId = Declaration.DeclarationId;
                Urn = Declaration.Urn;

                DateOfStatementOfIntentPublicationControl
                    = new Models.ThreePartDate(
                        source: DateTime.Parse(Declaration.DateOfStatementOfIntentPublication ?? DateTime.Today.ToShortDateString(), new CultureInfo("en-GB")),
                        title: ErrorMessagesExternalSite.DateOfStatementOfIntentPublication.WhatDateWasTheSoIPublished,
                        titleToBeUsedInErrorMessage: ErrorMessagesExternalSite.DateOfStatementOfIntentPublication.DateOfSoIPublished,
                        firstHint: "",
                        secondHint: ErrorMessagesExternalSite.SharedMessages.ForExample2522024,
                        showTheHighlightBar: true
                    );

            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.GetDeclaration, $"An issue occurred retrieving declaration data, {ex.Message}");
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.AnIssueOccurredRetrievingData;
            }
        }

        public string DateFromMin { get; set; } = Constants.SOI_MINIMUM_DATE;

        public string DateFromMax { get; set; } = DateTime.Now.ToString(Ofgem.LAF.SharedLibrary.Constants.DatetimeFormat.DATETIME_END_OF_DAY);


        public async Task<IActionResult> OnPost()
        {
            logger.LogInformation("DateOfStatementOfIntentPublicationModel - OnPost");

            try
            {
                Declaration = await declarationManagementService.GetDeclarationFromCacheAsync(DeclarationId)
                              ?? throw new ArgumentNullException($"{DeclarationId} does not exist");


                if (!ValidateDateOfStatementOfIntentPublicationControl())
                {
                    HasError = true;
                    return Page();
                }

                var dateOfStatementOfIntentPublication =
                    new DateTime(
                        DateOfStatementOfIntentPublicationControl.Year,
                        DateOfStatementOfIntentPublicationControl.Month,
                        DateOfStatementOfIntentPublicationControl.Day, 0, 0, 0, DateTimeKind.Utc);

                Declaration.DateOfStatementOfIntentPublication =
                    dateOfStatementOfIntentPublication.ToString("dd/MM/yyyy");

                declarationManagementService.UpdateDeclarationInCache(Declaration);

                return RedirectToPage(
                    LafPages.DeclarationDetail.ROUTE,
                    LafPages.DeclarationDetail.METHOD_USE_CACHED_DATA,
                    new
                    {
                        declarationId = DeclarationId
                    });

            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.GetDeclaration,
                    $"An issue occurred retrieving declaration data, {ex.Message}");
                DisplayMessage = ErrorMessagesExternalSite.SharedMessages.AnIssueOccurredRetrievingData;
            }

            return Page();
        }

        /// <summary>
        /// Validates the date control and sets the underlying model fields to the appropriate values
        /// </summary>
        /// <returns>true or false</returns>
        internal bool ValidateDateOfStatementOfIntentPublicationControl()
        {
            if (DateOfStatementOfIntentPublicationControl.HasErrors())
            {
                DisplayMessage = DateOfStatementOfIntentPublicationControl.ErrorMessage;
                return false;
            }

            var dateOfStatementOfIntent =
                new DateTime(
                    DateOfStatementOfIntentPublicationControl.Year,
                    DateOfStatementOfIntentPublicationControl.Month,
                    DateOfStatementOfIntentPublicationControl.Day, 0, 0, 0, DateTimeKind.Utc);

            if (dateOfStatementOfIntent < Convert.ToDateTime(DateFromMin, CultureInfo.InvariantCulture))
            {
                DisplayMessage = ErrorMessagesExternalSite.DateOfStatementOfIntentPublication.AValidDateMustHaveACorrectInputForYear;
                DateOfStatementOfIntentPublicationControl.HasError = true;
                DateOfStatementOfIntentPublicationControl.HasDayError = true;
                DateOfStatementOfIntentPublicationControl.HasMonthError = true;
                DateOfStatementOfIntentPublicationControl.HasYearError = true;
                DateOfStatementOfIntentPublicationControl.ErrorMessage = DisplayMessage;
                return false;
            }

            if (dateOfStatementOfIntent > Convert.ToDateTime(DateFromMax, CultureInfo.InvariantCulture))
            {
                DisplayMessage = $"{DateOfStatementOfIntentPublicationControl.TitleToBeUsedInErrorMessage} cannot be after {DateTime.Now:dd/MM/yyyy}";
                DateOfStatementOfIntentPublicationControl.HasError = true;
                DateOfStatementOfIntentPublicationControl.HasDayError = true;
                DateOfStatementOfIntentPublicationControl.HasMonthError = true;
                DateOfStatementOfIntentPublicationControl.HasYearError = true;
                DateOfStatementOfIntentPublicationControl.ErrorMessage = DisplayMessage;
                return false;
            }

            return true;
        }
    }
}
