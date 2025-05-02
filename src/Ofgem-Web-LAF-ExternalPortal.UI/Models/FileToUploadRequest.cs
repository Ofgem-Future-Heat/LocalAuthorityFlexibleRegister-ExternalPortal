namespace Ofgem_Web_LAF_ExternalPortal.Models;

public class FileToUploadRequest
{
    public Guid UserId { get; set; }
    public string? FileName { get; set; }
    public string? SupplierName { get; set; }
    public byte[]? ContentData { get; set; }
    public string? SubmissionNotes { get; set; }
}