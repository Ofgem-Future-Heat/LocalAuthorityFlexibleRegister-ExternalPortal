using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Constants;
using Ofgem.OneLogin.SharedLibrary.UserManagement.Models;

namespace Ofgem_Web_LAF_ExternalPortal.Pages;

[AutoValidateAntiforgeryToken]
[Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
[Authorize(Policy = ClaimsConstants.LinkedAccount)]
[Authorize(Policy = "MustHaveClaimLocalAuthorities")]

public class IndexModel(
    ILogger<IndexModel> logger,
    Services.IDeclarationManagementService declarationManagementService,
    Services.IUserManagementServiceUI userManagementService)
    : PageModel
{
    public Models.Claims? Claims { get; set; }

    [BindProperty] public List<Models.Declaration> Declarations { get; set; } = [];

    [BindProperty] public string? Message { get; set; } = string.Empty;

    [BindProperty] public string UserLaDetails { get; set; } = string.Empty;

    [BindProperty] public List<Models.AnnouncementView> PublishedAnnouncements { get; set; } = [];

    private const string ExcludedAnnouncementIdsKey = "ExcludedAnnouncementIds";

    public async Task OnGet()
    {
        logger.LogLafInformation(LogEvents.IndexGet);

        Claims = new Models.Claims(HttpContext);

        UserLaDetails = string.Join(", ", Claims.AsArrayOfStrings());

        await GetData();
    }

    public async Task OnGetHideAnnouncement(Guid id)
    {
        var excludedAnnouncementIdsBytes = HttpContext.Session.Get(ExcludedAnnouncementIdsKey);
        var excludesAnnouncementIdsJson = excludedAnnouncementIdsBytes == null || excludedAnnouncementIdsBytes.Length == 0 ? null : Encoding.ASCII.GetString(excludedAnnouncementIdsBytes);
        var excludedAnnouncementIds = (excludesAnnouncementIdsJson == null ? [] : JsonSerializer.Deserialize<HashSet<Guid>>(excludesAnnouncementIdsJson)) ?? [];

        excludedAnnouncementIds.Add(id);

        HttpContext.Session.Set(ExcludedAnnouncementIdsKey, Encoding.ASCII.GetBytes(JsonSerializer.Serialize(excludedAnnouncementIds)));

        await OnGet();
    }
        
    private async Task GetData()
    {
        try
        {
            Ofgem.LAF.SharedLibrary.Models.DeclarationFilter filter = new()
            {
                LAs = Claims?.AsArrayOfStrings() ?? [],
                PageIndex = 1,
                RecordsPerPage = 3,
                SortId = DeclarationSortOrder.DateUploadedZ2A,
                HasSubmissionErrors = true,
            };

            Declarations = await declarationManagementService.GetDeclarationsAsync(filter) ?? throw new InvalidOperationException("Unable to load the declaration data");

            if (!HttpContext.Session.TryGetValue(ExcludedAnnouncementIdsKey, out var excludedAnnouncementIdsBytes))
            {
                excludedAnnouncementIdsBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new HashSet<Guid>()));
                HttpContext.Session.Set(ExcludedAnnouncementIdsKey, excludedAnnouncementIdsBytes);
            }

            var excludedAnnouncementIds = JsonSerializer.Deserialize<HashSet<Guid>>(Encoding.ASCII.GetString(excludedAnnouncementIdsBytes))!;

            PublishedAnnouncements = (await userManagementService.GetPublishedAnnouncementsAsync())
                .Where(x => !excludedAnnouncementIds.Contains(x.AnnouncementId))
                .ToList() ?? throw new InvalidOperationException("Unable to load the announcements data");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetData failed {Message}", ex.Message);
        }
    }
}