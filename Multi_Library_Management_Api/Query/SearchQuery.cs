namespace Multi_Library_Management_Api.Query
{
    public class SearchQuery : PaginationQuery
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int? LibraryId { get; set; }
    }
}
