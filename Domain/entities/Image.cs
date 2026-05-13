namespace Domain.entities;

public class Image : IIdEntity
{
    public int? Id { get; }
    public int ProjectId { get; }
    public Project? Project { get; set; }
    public string FileName { get; }
    public string FileType { get; }
    public string RelationalFilePath { get; }

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
}