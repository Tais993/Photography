namespace Domain.entities.search;

public interface IPaginatedResult
{
    int PageNumber { get; }
    int PageSize { get; }
    int TotalItems { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}