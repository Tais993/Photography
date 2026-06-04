namespace Domain.entities;

public class ProjectMetadata : IEntity
{
    public ProjectMetadata(int projectId, string metadataValue, string metadataKey,
        string metadataType, string displayName, string description)
    {
        ProjectId = projectId;
        MetadataValue = metadataValue;
        MetadataKey = metadataKey;
        MetadataType = metadataType;
        DisplayName = displayName;
        Description = description;
    }

    public ProjectMetadata(int projectId, Metadata metadata, string metadataValue)
    {
        if (metadata.MetadataKey == null)
        {
            throw new ArgumentNullException("metadata", "metadata id cannot be null");
        }

        ProjectId = projectId;
        MetadataValue = metadataValue;
        MetadataKey = metadata.MetadataKey;
        MetadataType = metadata.MetadataType;
        DisplayName = metadata.DisplayName;
        Description = metadata.Description;
    }

    public ProjectMetadata(int projectId, string metadataKey, string? metadataValue)
    {
        ProjectId = projectId;
        MetadataKey = metadataKey;
        MetadataValue = metadataValue;
    }
    
    public int ProjectId { get; }
    public string? MetadataValue { get; set; }
    public string MetadataKey { get; }
    public string MetadataType { get; }
    public string DisplayName { get; }
    public string Description { get; }
}