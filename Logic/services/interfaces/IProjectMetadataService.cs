using Domain.entities;

namespace Application.services.interfaces;

public interface IProjectMetadataService
{
    public Metadata CreateMetadata(string metadataKey, string metadataType, string displayName, string description);
    public Metadata CreateMetadata(Metadata metadata);

    public void UpdateMetadata(string metadataKey, string metadataType, string displayName,
        string description);

    public void UpdateMetadata(Metadata metadata);

    public Metadata? GetMetadata(Metadata metadata);
    public Metadata? GetMetadata(string metadataKey);

    public void DeleteMetadata(Metadata metadata);
    public void DeleteMetadata(string metadataKey);


    public List<ProjectMetadata> GetProjectMetadata(Project project);
    public List<ProjectMetadata> GetProjectMetadata(int projectId);

    public void AddMetadataToProject(int projectId, string metadataKey, string? value);
    public void RemoveMetadataFromProject(int projectId, string metadataKey);
}