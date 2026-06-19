namespace Domain.entities;

public class ProjectFolder
{
    public ProjectFolder(ProjectFolderRole role, string metadataKey, string folderName, string relativePath, string absolutePath, bool exists)
    {
        Role = role;
        MetadataKey = metadataKey;
        FolderName = folderName;
        RelativePath = relativePath;
        AbsolutePath = absolutePath;
        Exists = exists;
    }

    public ProjectFolderRole Role { get; }
    public string MetadataKey { get; }
    public string FolderName { get; }
    public string RelativePath { get; }
    public string AbsolutePath { get; }
    public bool Exists { get; }
}