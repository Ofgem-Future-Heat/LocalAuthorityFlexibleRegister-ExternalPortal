using System.Net;
using Microsoft.AspNetCore.Authorization;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;
using Ofgem_Web_LAF_ExternalPortal.Exceptions;

namespace Ofgem_Web_LAF_ExternalPortal.Services;


public interface IDeclarationManagementService
{
    Task<List<Models.Declaration>?> GetDeclarationsAsync(Ofgem.LAF.SharedLibrary.Models.DeclarationFilter filter);

    Task<Models.DashboardView> GetPagedDeclarationsAsync(Ofgem.LAF.SharedLibrary.Models.DeclarationFilter filter);

    Task<Models.Declaration?> GetDeclarationAsync(Guid declarationId);

    Task<List<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>?> GetDownloadDataAsync(List<string> items);

    Task<Ofgem.LAF.SharedLibrary.Models.Upload?> CreateUpload(string container, string documentId, string createdByName);

    Task<Models.Declaration?> GetDeclarationFromCacheAsync(Guid declarationId);

    Task<Ofgem.LAF.SharedLibrary.Models.ValidationResponse?> SaveDeclarationAsync(Models.Declaration declaration);

    Task<Models.Declaration> RunCoreRulesAsync(Models.Declaration declaration);

    void UpdateDeclarationInCache(Models.Declaration declaration);

    void RemoveDeclarationFromCache(Guid declarationId);

    Task<Models.Declaration?> GetDeclarationByUrn(string urnNumber);
}


public class DeclarationManagementService(
    HttpClient httpClient,
    ILogger<DeclarationManagementService> logger,
    IHttpContextAccessor httpContextAccessor)
    : IDeclarationManagementService
{
    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<List<Models.Declaration>?> GetDeclarationsAsync(Ofgem.LAF.SharedLibrary.Models.DeclarationFilter filter)
    {

        List<Models.Declaration> declarations = [];

        try
        {

            var httpResponseMessage =
                await httpClient.PostAsJsonAsync(DeclarationApi.RouteGetFiltered,
                    filter);

            if (!httpResponseMessage.IsSuccessStatusCode) return null;

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.Declaration>>();

            if (result == null) return declarations;

            declarations = [];

            foreach (var item in result.Results)
            {
                if (item is null) continue;

                declarations.Add(Models.Declaration.MapFromDtoDeclaration(item));
            }

            return declarations;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Getting associated Local Authorities Failed. {Message}", ex.Message);
        }

        return declarations;
    }

    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<Models.DashboardView> GetPagedDeclarationsAsync(Ofgem.LAF.SharedLibrary.Models.DeclarationFilter filter)
    {

        var returnModel = new Models.DashboardView();

        try
        {
            var httpResponseMessage = await httpClient.PostAsJsonAsync(DeclarationApi.RouteGetFiltered, filter);

            if (!httpResponseMessage.IsSuccessStatusCode) return returnModel;

            var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.PagedResult<Ofgem.LAF.SharedLibrary.Models.Declaration>>();

            if (result == null) return returnModel;

            foreach (var item in result.Results)
            {
                returnModel.Declarations.Add(Models.Declaration.MapFromDtoDeclaration(item));
            }

            returnModel.CurrentPage = result.CurrentPage;
            returnModel.FirstRowOnPage = result.FirstRowOnPage;
            returnModel.LastRowOnPage = result.LastRowOnPage;
            returnModel.PageCount = result.PageCount;
            returnModel.PageSize = result.PageSize;
            returnModel.RowCount = result.RowCount;

            return returnModel;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDeclarations);
            throw;
        }
    }


    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<Models.Declaration?> GetDeclarationAsync(Guid declarationId)
    {
        logger.LogLafInformation(LogEvents.GetDeclaration, "DeclarationDetailModel - GetSupersededDeclarationsAsync");

        var declaration = new Models.Declaration();

        try
        {
            var httpResponseMessage = await httpClient.GetAsync($"{DeclarationApi.Route}/{declarationId}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var result = await httpResponseMessage.Content.ReadFromJsonAsync<Models.Declaration>();

                if (result == null) return declaration;

                declaration = result;

                declaration.DateOfHouseholderEligibility = declaration.DateOfHouseholderEligibilityForView;
                declaration.DateOfStatementOfIntentPublication = declaration.DateOfStatementOfIntentPublicationForView;

                return declaration;
            }
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDeclaration);
            throw;
        }

        return declaration;
    }

    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<List<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>?> GetDownloadDataAsync(List<string> items)
    {
        logger.LogLafInformation(LogEvents.GetDownloadData, items);

        List<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>? result = [];

        try
        {
            var httpResponseMessage =
                await httpClient.PostAsJsonAsync(DeclarationApi.RouteGetDownloadData, items);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Ofgem.LAF.SharedLibrary.Models.RawDeclarationWithErrors>>();

            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDownloadData);
            throw;
        }
    }


    [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
    public async Task<Ofgem.LAF.SharedLibrary.Models.Upload?> CreateUpload(string container, string documentId, string createdByName)
    {
        logger.LogLafInformation(LogEvents.CreateDocument, container, documentId);

        try
        {
            var upload = new Ofgem.LAF.SharedLibrary.Models.Upload()
            {
                SubmissionNotes = "External File Upload",
                DocumentContainerId = container,
                DocumentId = documentId,
                CreatedByName = createdByName
            };

            var httpResponseMessage =
                await httpClient.PostAsJsonAsync(DeclarationApi.RouteCreateUpload, upload);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                upload = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.Upload>();

            }
            else
            {
                throw new FailedToCreateUploadException();
            }

            return upload;
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.CreateDocument);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the declaration from the cache then the DB if not in the cache
    /// </summary>
    /// <param name="declarationId"></param>
    /// <returns></returns>
    public async Task<Models.Declaration?> GetDeclarationFromCacheAsync(Guid declarationId)
    {
        var cachedSource = DeclarationCacheService.Get(declarationId);

        if (cachedSource is not null) return cachedSource;

        var databaseSource = await GetDeclarationAsync(declarationId);

        DeclarationCacheService.Add(databaseSource);

        return databaseSource;
    }

    public async Task<Ofgem.LAF.SharedLibrary.Models.ValidationResponse?> SaveDeclarationAsync(Models.Declaration declaration)
    {
        if (httpContextAccessor.HttpContext != null)
        {
            var claims = new Models.Claims(httpContextAccessor.HttpContext);

            httpClient.DefaultRequestHeaders.Add("X-UserName", claims.EmailAddress);
        }

        var rawDeclaration = Models.Declaration.MapToRawDeclaration(declaration);

        var httpResponseMessage = await httpClient.PostAsJsonAsync(DeclarationApi.RouteSaveEditedDeclaration, rawDeclaration);

        if (!httpResponseMessage.IsSuccessStatusCode) return null;

        var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.ValidationResponse>();

        return result;
    }

    public async Task<Models.Declaration> RunCoreRulesAsync(Models.Declaration declaration)
    {
        if (httpContextAccessor.HttpContext != null)
        {
            var claims = new Models.Claims(httpContextAccessor.HttpContext);

            httpClient.DefaultRequestHeaders.Add("X-UserName", claims.EmailAddress);
        }

        var rawDeclaration = Models.Declaration.MapToRawDeclaration(declaration);

        var httpResponseMessage = await httpClient.PostAsJsonAsync(DeclarationApi.RouteRunCoreRules, rawDeclaration);

        if (!httpResponseMessage.IsSuccessStatusCode) throw new BadHttpRequestException("RunCoreRulesAsync - : Failed");

        var result = await httpResponseMessage.Content.ReadFromJsonAsync<Ofgem.LAF.SharedLibrary.Models.RunRulesResponse>()
                     ?? throw new BadHttpRequestException("RunCoreRulesAsync - : Failed no RunRulesResponse");

        if (result.Errors?.Count > 0)
        {
            result = RemoveDuplicateUrnError(result);
        }

        declaration.DeclarationErrors = Models.Declaration.MapToDeclarationError(result);

        return declaration;
    }

    private static Ofgem.LAF.SharedLibrary.Models.RunRulesResponse RemoveDuplicateUrnError(Ofgem.LAF.SharedLibrary.Models.RunRulesResponse rulesResponse)
    {
        foreach (var error in rulesResponse.Errors.ToList())
        {
            if (error.Rule == "DU_DECL_001_Urn_Is_Unique")
            {
                rulesResponse.Errors.Remove(error);
            }
        }

        return rulesResponse;
    }

    public void UpdateDeclarationInCache(Models.Declaration declaration)
    {
        DeclarationCacheService.Update(declaration);
    }

    public void RemoveDeclarationFromCache(Guid declarationId)
    {
        DeclarationCacheService.Remove(declarationId);
    }

    public async Task<Models.Declaration?> GetDeclarationByUrn(string urnNumber)
    {
        logger.LogLafInformation(LogEvents.GetDeclaration, "DeclarationService - SingleDeclaration - IsUrnExists");

        try
        {
            var httpResponseMessage = await httpClient.GetAsync($"{DeclarationApi.RouteIsUrnExists}/{urnNumber}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return httpResponseMessage.Content.Headers.ContentLength == 0
                    ? null
                    : await httpResponseMessage.Content.ReadFromJsonAsync<Models.Declaration?>();
            }

            var error = await httpResponseMessage.Content.ReadAsStringAsync();

            throw new Exception(error);
        }
        catch (Exception ex)
        {
            logger.LogLafError(ex, LogEvents.GetDeclaration);
            throw;
        }
    }
}