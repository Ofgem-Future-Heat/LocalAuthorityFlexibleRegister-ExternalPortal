using Microsoft.AspNetCore.Authorization;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Services;

public interface IDocumentManagementService
{
    Task<string?> CreateDocumentAsync(FileToUploadRequest request);

    Task<Models.Document?> GetDocumentAsync(string location);

}


public class DocumentManagementService(
    HttpClient httpClient,
    ILogger<DocumentManagementService> logger)
    : IDocumentManagementService
{

    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<string?> CreateDocumentAsync(FileToUploadRequest request)
    {
        logger.LogLafInformation(LogEvents.CreateDocument);

        try
        {
            var httpResponseMessage = await httpClient.PostAsJsonAsync($"{DocumentApi.Route}", request);

            httpResponseMessage.EnsureSuccessStatusCode();

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                logger.LogLafError(LogEvents.CreateDocument, request.FileName);
                return null;
            }

            if (httpResponseMessage.Headers.Location == null)
            {
                logger.LogLafError(LogEvents.CreateDocument, "Header is empty.", request.FileName);

                return (null);
            }

            return httpResponseMessage.Headers.Location.ToString();
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDeclaration);
            throw;
        }
    }

    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<Models.Document?> GetDocumentAsync(string location)
    {
        logger.LogLafInformation(LogEvents.GetDocument);

        try
        {
            var httpResponseMessage = await httpClient.GetAsync($"{location}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                logger.LogLafError(LogEvents.GetDocument, location);

                return null;
            }

            var documentResult = await httpResponseMessage.Content.ReadFromJsonAsync<Models.Document>();

            if (documentResult == null || documentResult.Description == null)
            {
                logger.LogLafError(LogEvents.GetDocument, location);
                return null;
            }

            return documentResult;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDocument);
            throw;
        }
    }
}