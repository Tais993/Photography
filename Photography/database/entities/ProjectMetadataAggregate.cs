namespace PhotographyNET.database.entities;

public class ProjectMetadataAggregate : IEntity
{
    public ProjectMetadataAggregate(int projectId, int metadataId, string metadataValue, string metadataKey,
        string metadataType, string displayName, string description)
    {
        ProjectId = projectId;
        MetadataId = metadataId;
        ProjectMetadataIds = new ProjectMetadataIds(projectId, metadataId);
        MetadataValue = metadataValue;
        MetadataKey = metadataKey;
        MetadataType = metadataType;
        DisplayName = displayName;
        Description = description;
    }

    public ProjectMetadataAggregate(ProjectMetadata projectMetadata, Metadata metadata) : this
        (projectMetadata.ProjectId, projectMetadata.MetadataId, projectMetadata.MetadataValue,
            metadata.MetadataKey, metadata.MetadataType, metadata.DisplayName, metadata.Description)
    {
    }

    public int ProjectId { get; }
    public int MetadataId { get; private set; }
    public ProjectMetadataIds ProjectMetadataIds { get; }
    public string MetadataValue { get; set; }
    public string MetadataKey { get; set; }
    public string MetadataType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }


    public Metadata ToMetadata() => new(MetadataId, MetadataKey, MetadataType, DisplayName, Description);

    public ProjectMetadata ToProjectMetadata() => new(ProjectId, MetadataId, MetadataValue);
}