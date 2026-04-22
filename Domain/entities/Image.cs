namespace Domain.entities;

public class Image(int id, int projectId, Project project, string fileName, string fileType, string filePath) : IIdEntity
{
    public int? Id { get; } = id;
    public int ProjectId { get; } = projectId;
    public Project? project { get; set; } = project;
    public string FileName { get; } = fileName;
    public string FileType { get; } = fileType;
    public string FilePath { get; } = filePath;
}