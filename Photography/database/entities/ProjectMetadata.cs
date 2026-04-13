namespace PhotographyNET.database.entities;

public class ProjectMetadata : IEntity
{
    public ProjectMetadata(int projectId, int metadataId, string metadataValue)
    {
        ProjectId = projectId;
        MetadataId = metadataId;
        ProjectMetadataIds = new ProjectMetadataIds(projectId, metadataId);
        MetadataValue = metadataValue;
    }

    public int ProjectId { get; }
    public int MetadataId { get; }
    public ProjectMetadataIds ProjectMetadataIds { get; }
    public string MetadataValue { get; }
}