using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    [BindProperties]
    public class DeclarationFilter
    {
        public string Address { get; set; } = string.Empty;

        public string Urn { get; set; } = string.Empty;

        public const int DateDaysDifference = 365;


        [BindProperty] public int DateFromDay { get; set; }
        [BindProperty] public int DateFromMonth { get; set; }
        [BindProperty] public int DateFromYear { get; set; }

        [BindProperty] public int DateToDay { get; set; }
        [BindProperty] public int DateToMonth { get; set; }
        [BindProperty] public int DateToYear { get; set; }

        public string DateFromMin { get; set; } = "2023-01-01";
        public string DateFromMax { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        public bool DateHasError { get; set; }
        public bool ToDateHasError { get; set; }
        public bool FromDateHasError { get; set; }
        public string DateErrorText { get; set; } = string.Empty;

        // Routes
        public bool Route1 { get; set; }
        public bool Route2 { get; set; }
        public bool Route3 { get; set; }
        public bool Route4 { get; set; }

        // Status
        public bool Submitted { get; set; }
        public bool SubmissionErrors { get; set; }

        public List<LocalAuthorityVm>? LAs { get; set; }

        public int PageIndex { get; set; }

        public List<SelectListItem> SortOrder =>
        [
            new()
            {
                Text = "Submitted status (Newest first)",
                Value = Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.SubmittedStatusNewestFirst.ToString()
            },
            new()
            {
                Text = "Submitted status (Oldest first)",
                Value = Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.SubmittedStatusOldestFirst.ToString()
            },
            new()
            {
                Text = "URN (Highest first)",
                Value = Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.UrnHighestFirst.ToString()
            },
            new()
            {
                Text = "URN (Lowest first)",
                Value = Ofgem.LAF.SharedLibrary.Enums.DeclarationSortOrder.UrnLowestFirst.ToString()
            }
        ];

        public string? SortOrderId { get; set; }
    }
}
