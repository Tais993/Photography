using Domain.entities;

namespace Application.interfaces.infrastructure.repositories;

public interface IProjectRepository
{
    Project? GetById(int id);
    List<Project> GetAll();
    Project Insert(Project project);
    void Update(Project project);
    public void UpdateStorage(Project project);
    void DeleteById(int id);

    public List<Project> GetAllByParentProjectId(int parentProjectId);
    int GetProjectCount();
}