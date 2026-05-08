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
        this.Id = id;
        this.ProjectId = projectId;
        this.Project = project;
        this.FileName = fileName;
        this.FileType = fileType;
        this.RelationalFilePath = relationalFilePath;
    }

    public Image(int projectId, string fileName, string fileType, string relationalFilePath)
    {
        this.ProjectId = projectId;
        this.FileName = fileName;
        this.FileType = fileType;
        this.RelationalFilePath = relationalFilePath;
    }




}