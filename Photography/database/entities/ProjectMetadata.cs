namespace PhotographyNET.database.entities;

public class ProjectMetadata : IEntity
{
    public ProjectMetadata(int projectId, int metadataId, string metadataValue, string metadataKey,
        string metadataType, string displayName, string description)
    {
        ProjectId = projectId;
        MetadataId = metadataId;
        MetadataValue = metadataValue;
        MetadataKey = metadataKey;
        MetadataType = metadataType;
        DisplayName = displayName;
        Description = description;
    }

    public int ProjectId { get; }
    public int MetadataId { get; }
    public string MetadataValue { get; set; }
    public string MetadataKey { get; }
    public string MetadataType { get; }
    public string DisplayName { get; }
    public string Description { get; }
}