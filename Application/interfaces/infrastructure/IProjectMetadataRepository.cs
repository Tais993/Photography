using Domain.entities;

namespace Application.interfaces;

public interface IProjectMetadataRepository
{
    List<ProjectMetadata> GetAll();
    void Insert(ProjectMetadata entity);
    void Update(ProjectMetadata entity);
    ProjectMetadata? GetByKey(int projectId, string metadataKey);
    void DeleteByKey(int projectId, string metadataKey);
    List<ProjectMetadata> GetAllByProjectId(int projectId);
}