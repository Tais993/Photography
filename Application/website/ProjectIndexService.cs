using Application.services;
using Application.services.interfaces;
using Application.website.interfaces;
using Domain.entities;
using Domain.entities.search;
using Domain.website;

namespace Application.website;

public class ProjectIndexService : IProjectIndexService
{
    private readonly IProjectService _projectService;
    private readonly ISearchService _searchService;

    public ProjectIndexService(IProjectService projectService, ISearchService searchService)
    {
        _projectService = projectService;
        _searchService = searchService;
    }

    public ProjectIndexViewModel GetProjectIndex(ProjectIndexRequest request)
    {
        IEnumerable<Project> projects = _searchService.SearchProjects(new ProjectSearchSettings()
        {
            EventDate = request.EventDate,
            ProjectName = request.Search,
            ProjectPath = request.ProjectPath,
            ProjectId = request.ProjectId,
            ParentProjectId = request.ParentProjectId
        });

        List<Project> orderedProjects = projects
            .OrderByDescending(project => project.EventDate)
            .ThenBy(project => project.Name)
            .ToList();

        PaginatedResult<Project> projectPage = PaginationService.Paginate(
            orderedProjects,
            new ProjectSearchSettings
            {
                PageNumber = request.ProjectPageNumber,
                PageSize = request.ProjectPageSize
            });

        ProjectIndexViewModel viewModel = new()
        {
            ProjectPage = projectPage,
            Projects = projectPage.Items,
            SelectedProjectId = request.SelectedProjectId,
            ProjectCount = _projectService.GetProjectCount(),
            ProjectPageNumber = projectPage.PageNumber,
            ProjectPageSize = projectPage.PageSize
        };

        if (request.SelectedProjectId is not null)
        {
            viewModel.SelectedProject = _projectService.GetProjectById(request.SelectedProjectId.Value);

            return viewModel;
        }

        viewModel.SelectedProject = projectPage.Items.FirstOrDefault();
        viewModel.SelectedProjectId = viewModel.SelectedProject?.Id;

        return viewModel;
    }

    public ProjectViewModel CreateProjectView(Project project, int? selectedProjectId)
    {
        return new ProjectViewModel()
        {
            Selected = project.Id == selectedProjectId,
            Id = project.Id,
            Name = project.Name,
            Path = project.Path,
            EventDate = project.EventDate
        };
    }
}