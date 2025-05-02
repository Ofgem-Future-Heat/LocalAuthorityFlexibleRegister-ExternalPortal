namespace Ofgem_Web_LAF_ExternalPortal.Models;

public class AnnouncementView
{
    public Guid AnnouncementId { get; set; }

    public string? Headline { get; set; }

    public string? Text { get; set; }

    public bool PublishNow { get; set; }

    public DateTime PublishedDate { get; set; }

    public bool DoNotUnPublish { get; set; }

    public DateTime? UnPublishedDate { get; set; }

    public DateTime? ActionDate { get; set; }

    public string PublishedSummary =>
        $"Posted: {PublishedDate:h:mm tt, dddd dd/MM/yyyy}";

    public bool Show { get; set; }

    public static AnnouncementView MapFromDtoDeclaration(Ofgem.LAF.SharedLibrary.Models.Announcement item)
        => new()
        {
            AnnouncementId = item.AnnouncementId,
            Headline = item.Headline,
            Text = item.Text,
            PublishNow = item.PublishNow,
            PublishedDate = item.PublishedDate,
            DoNotUnPublish = item.DoNotUnPublish,
            UnPublishedDate = item.UnPublishedDate,
            ActionDate = item.ActionDate
        };
}