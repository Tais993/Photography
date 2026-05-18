using Domain.entities;

namespace Application.services.interfaces;

public interface IProjectMetadataService
{
    public Metadata CreateMetadata(string metadataKey, string metadataType, string displayName, string description);
    public Metadata CreateMetadata(Metadata metadata);

    public Metadata? GetMetadata(Metadata metadata);
    public Metadata? GetMetadata(int metadataId);

    public void DeleteMetadata(Metadata metadata);
    public void DeleteMetadata(int metadataId);


    public List<ProjectMetadata> GetProjectMetadata(Project project);
    public List<ProjectMetadata> GetProjectMetadata(int projectId);

    public void AddMetadataToProject(int projectId, int metadataId, string value);
    public void DeleteMetadataFromProject(int projectId, int metadataId);
}