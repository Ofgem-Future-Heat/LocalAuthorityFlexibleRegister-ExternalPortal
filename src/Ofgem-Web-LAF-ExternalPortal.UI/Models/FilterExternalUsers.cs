using Microsoft.AspNetCore.Mvc;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    [BindProperties]
    public class FilterExternalUsers : FilterBase
    {
        public string OnsCode { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"PageIndex: {PageIndex}, RecordsPerPage: {RecordsPerPage}, Filter: {Filter}";
        }
    }
}
