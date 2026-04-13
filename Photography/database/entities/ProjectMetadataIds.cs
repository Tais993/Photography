namespace PhotographyNET.database.entities;

public class ProjectMetadataIds
{
    public int ProjectId { get; }
    public int MetadataId { get; }

    public ProjectMetadataIds(int projectId, int metadataId)
    {
        ProjectId = projectId;
        MetadataId = metadataId;
    }

    public ProjectMetadataIds(ProjectMetadata projectMetadata) : this(projectMetadata.ProjectId, projectMetadata.MetadataId)
    {
    }
}