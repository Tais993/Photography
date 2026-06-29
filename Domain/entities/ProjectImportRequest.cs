namespace Domain.entities;

public class ProjectImportRequest
{
    public int ProjectId { get; set; }
    public IList<string> FilePaths { get; set; } = [];
    public bool RemoveSourceFilesAfterImport { get; set; }
}