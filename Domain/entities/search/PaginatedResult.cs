namespace Domain.entities.search;

public class PaginatedResult<T>
{
    public static PaginatedResult<T> Empty = new PaginatedResult<T>();
    
    
    public List<T> Items { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

}