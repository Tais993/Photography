namespace Domain.entities;

public class Image
{

    public Image(int? id, int projectId, Project? project, string fileName, string fileType, string relationalFilePath)
    {
        Id = id;
        ProjectId = projectId;
        Project = project;
        FileName = fileName;
        FileType = fileType;
        RelationalFilePath = relationalFilePath;
    }

    public Image(int projectId, string fileName, string fileType, string relationalFilePath)
    {
        ProjectId = projectId;
        FileName = fileName;
        FileType = fileType;
        RelationalFilePath = relationalFilePath;
    }

    public int? Id { get; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
    public string RelationalFilePath { get; set; }
}