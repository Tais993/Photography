namespace Domain.website;

public class ProjectIndexRequest
{
    public int? SelectedProjectId { get; set; }
    public string? Search { get; set; }
    public int? ProjectId { get; set; }
    public int? ParentProjectId { get; set; }
    public string? ProjectPath { get; set; }
    public DateOnly? EventDate { get; set; }

    public int ProjectPageNumber { get; set; }
    public int ProjectPageSize { get; set; }
}