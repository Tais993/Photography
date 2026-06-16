using Domain.entities.search;

namespace Application.services;

public static class PaginationService
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 20;

    public static PaginatedResult<T> Paginate<T>(IEnumerable<T> items, SearchSettings settings)
    {
        List<T> itemList = items.ToList();

        int totalItems = itemList.Count;

        int pageSize = settings.PageSize <= 0
            ? DefaultPageSize
            : settings.PageSize;

        int totalPages = pageSize <= 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        int pageNumber = settings.PageNumber <= 0
            ? DefaultPageNumber
            : settings.PageNumber;

        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        List<T> paginatedItems = itemList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<T>
        {
            Items = paginatedItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}