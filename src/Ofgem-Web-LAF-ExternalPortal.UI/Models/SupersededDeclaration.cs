namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    public class SupersededDeclaration
    {
        public Guid DeclarationId { get; set; }
        public string? Urn { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Version {  get; set; }
        
    }
}
