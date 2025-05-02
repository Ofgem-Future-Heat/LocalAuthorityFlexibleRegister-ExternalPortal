namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    public static class Pagination
    {
        /// <summary>
        /// GDS compliant pagination, Use ellipses (…) to replace any skipped pages. For example:
        /// 
        /// [1] 2 … 100
        /// 1 [2] 3 … 100
        /// 1 2 [3] 4 … 100
        /// 1 2 3 [4] 5 … 100
        /// 1 … 4 [5] 6 … 100
        /// 1 … 97 [98] 99 100
        /// 1 … 98 [99] 100
        /// 1 … 99 [100]
        /// 
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>

        public static List<string> GeneratePaginationList(int currentPage, int pageCount)
        {
            var newPagination = new List<string>();

            var pageNumbers = new List<int>();

            for (int i = 1; i < pageCount + 1; i++)
            {
                pageNumbers.Add(i);
            }

            var hasFirstDots = false;
            var hasLastDots = false;
            var lastPage = pageNumbers[^1];

            foreach (int pageNumber in pageNumbers)
            {
                if (pageNumber == 1)
                {
                    newPagination.Add(pageNumber.ToString());
                    continue;
                }

                if (pageNumber == lastPage)
                {
                    newPagination.Add(pageNumber.ToString());
                    continue;
                }

                if (pageNumber <= currentPage)
                {
                    if (pageNumber == currentPage - 1)
                    {
                        newPagination.Add(pageNumber.ToString());
                    }
                    else if
                        (pageNumber == currentPage)
                    {
                        newPagination.Add(pageNumber.ToString());
                    }
                    else
                    {
                        if (hasFirstDots) continue;

                        newPagination.Add("...");
                        hasFirstDots = true;
                    }
                }

                if (pageNumber <= currentPage) continue;

                if (pageNumber == currentPage + 1)
                {
                    newPagination.Add(pageNumber.ToString());
                }
                else
                {
                    if (hasLastDots) continue;

                    newPagination.Add("...");
                    hasLastDots = true;
                }
            }

            return newPagination;
        }
    }
}
