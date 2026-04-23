using Domain.entities;

namespace Infrastructure.database.repositories;

public interface IProjectRepository
{
    Project GetByKey(int id);
    List<Project> GetAll();
    Project Insert(Project project);
    void Update(Project project);
    void DeleteByKey(int id);
}