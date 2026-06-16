using Domain.entities;
using Domain.entities.search;

namespace Domain.website;

public class ProjectIndexViewModel
{
    public PaginatedResult<Project> ProjectPage = PaginatedResult<Project>.Empty;
    
    public List<Project> Projects = [];
    public Project? SelectedProject;
    public int? SelectedProjectId;
    public int ProjectCount;

    public int ProjectPageNumber;
    public int ProjectPageSize;
}