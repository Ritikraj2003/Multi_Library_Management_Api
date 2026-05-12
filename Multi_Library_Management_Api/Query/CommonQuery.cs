using Multi_Library_Management_Api.Models;

namespace Multi_Library_Management_Api.Query
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    public static class CommonQuery
    {
        /// <summary>Apply pagination to any IQueryable source.</summary>
        public static IQueryable<T> ApplyPagination<T>(
            IQueryable<T> query, int pageNumber, int pageSize)
        {
            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>Build a PagedResult from total count and paginated items.</summary>
        public static PagedResult<T> BuildPagedResult<T>(
            List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
