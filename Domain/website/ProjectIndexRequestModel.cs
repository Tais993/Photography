namespace Domain.website;

public class ProjectIndexRequest
{
    public int? SelectedProjectId;
    public string? Search;
    public int? ProjectId;
    public int? ParentProjectId;
    public string? ProjectPath;
    public DateOnly? EventDate;
    
    public int ProjectPageNumber;
    public int ProjectPageSize;
}
