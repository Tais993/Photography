using Domain.entities;

namespace Application.interfaces.services.metadata;

public interface IProjectMetadataService
{
    public List<ProjectMetadata> GetProjectMetadata(Project project);
    public List<ProjectMetadata> GetProjectMetadata(int projectId);
    public ProjectMetadata? GetProjectMetadata(Project project, string metadataKey);
    public ProjectMetadata? GetProjectMetadata(int projectId, string metadataKey);

    public void AddMetadataToProject(int projectId, string metadataKey, string? value);
    public void RemoveMetadataFromProject(int projectId, string metadataKey);
}