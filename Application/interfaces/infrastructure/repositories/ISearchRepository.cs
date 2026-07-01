using Domain.entities;
using Domain.entities.search;

namespace Application.interfaces.infrastructure.repositories;

public interface ISearchRepository
{
    int CountImages(ImageSearchSettings imageSearchSettings);
    List<Image> SearchImages(ImageSearchSettings imageSearchSettings);

    int CountProjects(ProjectSearchSettings projectSearchSettings);
    List<Project> SearchProjects(ProjectSearchSettings projectSearchSettings);
}