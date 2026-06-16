using Domain.entities;
using Domain.website;

namespace Application.website.interfaces;

public interface IProjectIndexService
{
    ProjectIndexViewModel GetProjectIndex(ProjectIndexRequest request);
    ProjectViewModel CreateProjectView(Project project, int? selectedProjectId);
}