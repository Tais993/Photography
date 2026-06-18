namespace Domain.entities.search;

public class PaginatedResult<T> : IPaginatedResult
{
    public List<T> Items { get; set; } = [];
    public int TotalItems { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResult<T> Empty => new()
    {
        Items = [],
        TotalItems = 0,
        PageNumber = 1,
        PageSize = 20
    };
}