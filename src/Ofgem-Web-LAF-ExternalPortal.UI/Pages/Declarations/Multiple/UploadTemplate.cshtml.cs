using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Constants;
using Ofgem.LAF.SharedLibrary.Models.Messaging;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Exceptions;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem_Web_LAF_ExternalPortal.Services;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.Declarations.Multiple
{
    [AutoValidateAntiforgeryToken]
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    [Authorize(Policy = ClaimsConstants.LinkedAccount)]
    [Authorize(Policy = "MustHaveClaimLocalAuthorities")]
    public class UploadTemplateModel(
        IDocumentManagementService documentManagementService,
        IDeclarationManagementService declarationManagementService,
        ISendMessageService sendMessageService,
        IUserManagementServiceUI userManagementServiceUi,
        ILogger<UploadTemplateModel> logger,
        IRedactionService redactionService)
        : PageModel
    {
        public Models.Claims? Claims { get; set; }

        [BindProperty]
        public IFormFile? SelectedUploadFile { get; set; }

        [BindProperty]
        public List<string?> UploadErrors { get; set; } = [];

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        [BindProperty] public bool HasMessage => Message.Length > 0;

        [BindProperty] public bool HasUploadError => UploadErrors.Count > 0;

        private const string CSV_SIZE = ErrorMessagesExternalSite.UploadTemplate.TheCsvMustBeSmallerThan2Mb;

        public void OnGet()
        {
            logger.LogLafInformation(LogEvents.UploadDocument);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            logger.LogLafInformation(LogEvents.UploadDocument);

            var claims = new Models.Claims(HttpContext);

            ArgumentNullException.ThrowIfNull(claims);

            Message = string.Empty;
            UploadErrors = [];

            var result = await UploadDocumentToDocumentStore(claims);

            if (!result.Item1)
            {
                UploadErrors.Add(result.Item2);
                return Page();
            }

            if (result.Item2 is null)
            {
                throw new MissingDocumentInformationException();
            }

            var document = result.Item2;
            string[] items = document.Split("/");
            var fileContainer = items[0];
            var fileName = items[1];


            _ = await declarationManagementService.CreateUpload(fileContainer, fileName, claims.EmailAddress);

            return RedirectToPage(LafPages.UploadTemplateConfirmation.ROUTE);
        }

        private async Task<(bool, string?)> UploadDocumentToDocumentStore(Models.Claims claims)
        {
            logger.LogLafInformation(LogEvents.UploadDocument);

            ArgumentNullException.ThrowIfNull(claims);

            Message = string.Empty;
            UploadErrors = [];

            if (SelectedUploadFile == null)
            {
                return (false, "Select an LA Flex file");
            }

            if (!SelectedUploadFile.FileName.EndsWith(".csv"))
            {
                return (false, "The selected file must be a CSV.");
            }

            if (SelectedUploadFile.Length > 2048000)
            {
                return (false, CSV_SIZE);
            }

            using var memoryStream = new MemoryStream();
            try
            {
                await SelectedUploadFile.CopyToAsync(memoryStream);

                var fileBytes = memoryStream.ToArray();

                var file = new Models.FileToUploadRequest
                {
                    UserId = Guid.NewGuid(), // TODO Permissions.UserId,
                    FileName = SelectedUploadFile.FileName,
                    ContentData = redactionService.RedactPersonalInformation(fileBytes),
                    SubmissionNotes = string.Empty,
                    SupplierName = string.Empty
                };

                var location = await documentManagementService.CreateDocumentAsync(file);

                ArgumentNullException.ThrowIfNull(location);

                var document = await documentManagementService.GetDocumentAsync(location);

                ArgumentNullException.ThrowIfNull(document);

                if (document.Description is null)
                {
                    logger.LogLafError(LogEvents.GetDocument,
                        "The document could not be retrieved from the service.");

                    return (false, "The selected file could not be uploaded – try again");
                }

                // get the DAS email and the managing la officer emails.

                if (claims.BaseLocalAuthority is null)
                {
                    logger.LogLafError(LogEvents.GetDocument,
                        "The users Claims do not included a BaseLocalAuthority.");

                    return (false, "The selected file has been uploaded, unfortunately the confirmation email cannot be sent");
                }

                if (claims.BaseLocalAuthority.OnsCode is null)
                {
                    logger.LogLafError(LogEvents.GetDocument,
                        "The users Claims do not included a BaseLocalAuthority OnsCode.");

                    return (false, "The selected file has been uploaded, unfortunately the confirmation email cannot be sent");
                }

                if (claims.EmailAddress is null)
                {
                    logger.LogLafError(LogEvents.GetDocument,
                        "The users Claims do not included a EmailAddress.");

                    return (false, "The selected file has been uploaded, unfortunately the confirmation email cannot be sent");
                }

                var dasUser = await userManagementServiceUi.GetDesignatedAuthorisedSignatory(
                    claims.BaseLocalAuthority.OnsCode);

                var laManagers = await userManagementServiceUi.GetManagingLaOfficers(claims.BaseLocalAuthority.OnsCode);

                var splitDescription = document.Description.Split('/');

                // document has uploaded send message to do the malware check
                await sendMessageService.SendMessage(new MalwareCheckMessage()
                {
                    Container = splitDescription[0],
                    DocumentId = splitDescription[1],
                    EmailAddress = claims.EmailAddress,
                    LocalAuthorities = string.Join(",", claims.AsArrayOfStrings()),
                    DasEmail = dasUser?.EmailAddress ?? string.Empty,
                    LaManagerEmails = laManagers.Select(u => u?.EmailAddress ?? string.Empty ).ToList()
                });

                return (true, document.Description);
            }
            catch (Exception ex)
            {
                logger.LogLafError(ex, LogEvents.GetDocument,
                    "Upload declarations to documents service error.");
                return (false, "The selected file could not be uploaded – try again");
            }
        }
    }
}
