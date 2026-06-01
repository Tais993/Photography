using Domain.entities;

namespace Infrastructure.database.repositories;

public interface IProjectMetadataRepository
{
    List<ProjectMetadata> GetAll();
    void Insert(ProjectMetadata entity);
    void Update(ProjectMetadata entity);
    ProjectMetadata? GetById(int projectId, int metadataId);
    void DeleteByKey(int projectId, int metadataId);
    List<ProjectMetadata> GetAllByProjectId(int projectId);
}