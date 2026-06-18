using Domain.entities;

namespace Application.interfaces.infrastructure;

public interface IProjectRepository
{
    Project? GetById(int id);
    List<Project> GetAll();
    Project Insert(Project project);
    void Update(Project project);
    void DeleteById(int id);

    int GetProjectCount();
}