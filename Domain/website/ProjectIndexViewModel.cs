using Domain.entities;
using Domain.entities.search;

namespace Domain.website;

public class ProjectIndexViewModel
{
    public PaginatedResult<Project> ProjectPage = PaginatedResult<Project>.Empty;

    public List<Project> Projects { get; init; } = [];
    public Project? SelectedProject { get; set; }
    public int? SelectedProjectId { get; set; }
    public int ProjectCount { get; init; }

    public int ProjectPageNumber { get; init; }
    public int ProjectPageSize { get; init; }

    public bool CanOpenProjectInImageViewer { get; init; }
    public string ImageViewerName { get; init; } = "image viewer";
}