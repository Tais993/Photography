namespace Domain.entities.search;

public abstract class SearchSettings
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 60;
}