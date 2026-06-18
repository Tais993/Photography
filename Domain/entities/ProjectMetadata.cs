namespace Domain.entities;

public class ProjectMetadata
{
    public ProjectMetadata(int projectId, string metadataKey, string? metadataValue, string metadataType, string displayName, string description)
    {
        ProjectId = projectId;
        MetadataKey = metadataKey;
        MetadataValue = metadataValue;
        MetadataType = metadataType;
        DisplayName = displayName;
        Description = description;
    }

    public ProjectMetadata(
        int projectId,
        Metadata metadata,
        string? metadataValue)
    {
        ProjectId = projectId;
        MetadataKey = metadata.MetadataKey;
        MetadataValue = metadataValue;
        MetadataType = metadata.MetadataType;
        DisplayName = metadata.DisplayName;
        Description = metadata.Description;
    }

    public ProjectMetadata(
        int projectId,
        string metadataKey,
        string? metadataValue)
    {
        ProjectId = projectId;
        MetadataKey = metadataKey;
        MetadataValue = metadataValue;

        MetadataType = string.Empty;
        DisplayName = string.Empty;
        Description = string.Empty;
    }

    public int ProjectId { get; }
    public string MetadataKey { get; }
    public string? MetadataValue { get; set; }
    public string MetadataType { get; }
    public string DisplayName { get; }
    public string Description { get; }
}