namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    public class DeclarationNote
    {
        public Guid DeclarationNoteId { get; set; }
        public Guid DeclarationId { get; set; }
        public string? Text { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public string? UserName { get; set; }
    }
}
