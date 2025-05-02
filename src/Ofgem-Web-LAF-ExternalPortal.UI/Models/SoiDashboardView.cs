namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    public class SoiDashboardView
    {
        public List<Models.StatementOfIntent> StatementOfIntents { get; set; } = [];

        public int CurrentPage { get; set; } = 0;
        public int PageCount { get; set; } = 0;
        public int PageSize { get; set; } = 0;
        public int RowCount { get; set; } = 0;
        public int FirstRowOnPage { get; set; } = 0;
        public int LastRowOnPage { get; set; } = 0;
    }
}
