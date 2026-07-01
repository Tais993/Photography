using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Application.interfaces.website;
using Domain.entities;
using Domain.entities.search;
using Domain.website;
using Microsoft.Extensions.Logging;

namespace Application.website;

public class ProjectIndexService : IProjectIndexService
{
    private readonly IProjectService _projectService;
    private readonly IProjectFolderService _projectFolderService;
    private readonly ISearchService _searchService;
    private readonly IFiles _files;
    private readonly IImageViewerService _imageViewerService;
    private readonly ILogger<ProjectIndexService> _logger;


    public ProjectIndexService(
        IProjectService projectService,
        IProjectFolderService projectFolderService,
        ISearchService searchService,
        IImageViewerService imageViewerService,
        ILogger<ProjectIndexService> logger, IFiles files)
    {
        _projectService = projectService;
        _projectFolderService = projectFolderService;
        _searchService = searchService;
        _imageViewerService = imageViewerService;
        _logger = logger;
        _files = files;
        _imageViewerService = imageViewerService;
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
            ProjectPageSize = projectPage.PageSize,
            CanOpenProjectInImageViewer = _imageViewerService.IsAvailable() && _imageViewerService.CanOpenFolders(),
            ImageViewerName = _imageViewerService.GetImageViewerName()
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
            EventDate = project.EventDate,

            StorageTotalBytes = project.StorageTotalBytes,
            StorageLocalBytes = project.StorageLocalBytes
        };
    }

    public SelectedProjectViewModel CreateSelectedProjectView(Project project)
    {
        return new SelectedProjectViewModel()
        {
            Project = project,
            CanOpenInImageViewer = _imageViewerService.IsAvailable() && _imageViewerService.CanOpenFolders(),
            ImageViewerName = _imageViewerService.GetImageViewerName()
        };
    }

    public void OpenProjectFolder(int projectId)
    {
        _logger.LogInformation("Opening project images in image viewer, project: {ProjectId}", projectId);

        Project? project = _projectService.GetProjectById(projectId);

        if (project is null)
        {
            _logger.LogWarning("Could not open project images because project was not found: {ProjectId}", projectId);
            throw new InvalidOperationException("Project was not found.");
        }

        if (!_imageViewerService.IsAvailable() || !_imageViewerService.CanOpenFolders())
        {
            _logger.LogWarning("Could not open project images. Image viewer is not available, or does not support opening folders");
            throw new InvalidOperationException("Image viewer is not available, or does not support opening folders.");
        }


        string folderPath = _projectFolderService.GetRequiredFolderPath(projectId, ProjectFolderRole.Originals);

        _logger.LogInformation(
            "Opening project images folder in {ImageViewerName}: {FolderPath}",
            _imageViewerService.GetImageViewerName(),
            folderPath);

        _imageViewerService.OpenProjectFolder(folderPath);
    }

    public void OpenProjectCommandLine(int projectId)
    {
        _logger.LogInformation("Opening project in command line, project: {ProjectId}", projectId);
        
        Project? project = _projectService.GetProjectById(projectId) ?? throw new InvalidOperationException("Project was not found.");
        string path = project.Path;

        _files.OpenCommandLine(path);
    }
}