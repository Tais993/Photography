using Domain.entities.search;

namespace Application.services;

public class PaginationService
{
    public static PaginatedResult<T> Paginate<T>(IEnumerable<T> items, SearchSettings searchSettings)
    {
        int pageNumber = searchSettings.PageNumber;
        int pageSize = searchSettings.PageSize;

        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 60;
        }

        List<T> itemList = items.ToList();

        int totalItems = itemList.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        if (pageNumber > totalPages && totalPages > 0)
        {
            pageNumber = totalPages;
        }

        List<T> pagedItems = itemList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<T>
        {
            Items = pagedItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}