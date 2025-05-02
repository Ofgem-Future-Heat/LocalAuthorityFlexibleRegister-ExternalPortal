#pragma warning disable CA1707 // Identifiers should not contain underscores

using Microsoft.AspNetCore.Mvc;
using Ofgem_Web_LAF_ExternalPortal.Extensions;

namespace Ofgem_Web_LAF_ExternalPortal.Models
{
    [BindProperties]
    public class PagePagination
    {
        public const int PREVIOUS_PAGE_VALUE = -1;
        public const int NEXT_PAGE_VALUE = 999;
        public const string NEXT = "Next";
        public const string PREVIOUS = "Previous";


        public bool ShowPrevious { get; set; }
        public List<string>? PageNumbers { get; set; }
        public bool ShowNext { get; set; }
        public string? CurrentPage { get; set; }
        public string? Message { get; set; }
        public bool NoDataFound { get; set; }

        public PagePagination() { }

        public PagePagination(int recordCount, int currentPage, int pageCount)
        {
            // no data
            if (recordCount == 0)
            {
                ShowNext = false;
                ShowPrevious = false;
                PageNumbers = new List<string>();
                CurrentPage = string.Empty;
                Message = "No data found";
                NoDataFound = true;

                return;
            }

            // single page
            if (pageCount == 1)
            {
                ShowNext = false;
                ShowPrevious = false;
                PageNumbers = new List<string>() { "1" };
                CurrentPage = "1";
                Message = string.Empty;
                NoDataFound = false;

                return;
            }

            // multiple pages
            Message = string.Empty;
            ShowPrevious = true;
            ShowNext = true;

            CurrentPage = currentPage.ToString();

            PageNumbers = Pagination.GeneratePaginationList(currentPage, pageCount);

            if (PageNumbers!.IndexOf(CurrentPage) == 0)
            {
                ShowPrevious = false;
            }

            if (PageNumbers.IndexOf(CurrentPage) == PageNumbers.Count - 1)
            {
                ShowNext = false;
            }
        }
    }
}
