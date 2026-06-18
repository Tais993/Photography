using Application.interfaces.services;
using Application.interfaces.website;
using Domain.entities;
using Domain.entities.search;
using Domain.website;
using Microsoft.Extensions.Logging;

namespace Application.website;

public class ProjectIndexService : IProjectIndexService
{
    private readonly IProjectService _projectService;
    private readonly ISearchService _searchService;
    private readonly ILogger<ProjectIndexService> _logger;

    public ProjectIndexService(
        IProjectService projectService,
        ISearchService searchService,
        ILogger<ProjectIndexService> logger)
    {
        _projectService = projectService;
        _searchService = searchService;
        _logger = logger;
    }

    public ProjectIndexViewModel GetProjectIndex(ProjectIndexRequest request)
    {
        _logger.LogDebug("Creating project index view model");

        PaginatedResult<Project> projectPage = _searchService.SearchProjects(new ProjectSearchSettings
        {
            EventDate = request.EventDate,
            ProjectName = request.Search,
            ProjectPath = request.ProjectPath,
            ProjectId = request.ProjectId,
            ParentProjectId = request.ParentProjectId,
            PageNumber = request.ProjectPageNumber,
            PageSize = request.ProjectPageSize
        });

        _logger.LogDebug("Found {Count} projects for project index", projectPage.TotalItems);

        _logger.LogInformation(
            "Created project page. Page number: {PageNumber}, page size: {PageSize}, total items: {TotalItems}",
            projectPage.PageNumber,
            projectPage.PageSize,
            projectPage.TotalItems);

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
            _logger.LogDebug("Project index selected project provided by request: {ProjectId}", request.SelectedProjectId);
            viewModel.SelectedProject = _projectService.GetProjectById(request.SelectedProjectId.Value);

            return viewModel;
        }

        viewModel.SelectedProject = projectPage.Items.FirstOrDefault();
        viewModel.SelectedProjectId = viewModel.SelectedProject?.Id;

        _logger.LogDebug("Project index selected project resolved to: {ProjectId}", viewModel.SelectedProjectId);

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