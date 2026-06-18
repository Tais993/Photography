namespace Domain.entities;

public class Image
{
    public Image(int projectId, string fileName, string fileType, string relationalFilePath, int? id = null, Project? project = null)
    {
        ProjectId = projectId;
        FileName = fileName;
        FileType = fileType;
        RelationalFilePath = relationalFilePath;
        Project = project;
        Id = id;
    }

    public int? Id { get; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
    public string RelationalFilePath { get; set; }
}