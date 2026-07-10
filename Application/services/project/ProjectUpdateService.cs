using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services.project;

/// <summary>
/// This service scans the project, updates any metadata folders, and updates all storage information.
/// Additionally, the folder location gets updated in the database
/// </summary>
public class ProjectUpdateService : IProjectUpdateService
{
    
    private readonly IProjectFileScanningService _projectFileScanningService;
    private readonly IProjectStorageService _projectStorageService;
    private readonly IProjectFolderService _projectFolderService;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectResolverService _projectResolverService;
    private readonly ILogger<ProjectUpdateService> _logger;

    public ProjectUpdateService(IProjectFileScanningService projectFileScanningService, IProjectStorageService projectStorageService,
        IProjectRepository projectRepository, ILogger<ProjectUpdateService> logger, IProjectFolderService projectFolderService, IProjectResolverService projectResolverService)
    {
        _projectFileScanningService = projectFileScanningService;
        _projectStorageService = projectStorageService;
        _projectRepository = projectRepository;
        _logger = logger;
        _projectFolderService = projectFolderService;
        _projectResolverService = projectResolverService;
    }

    public void UpdateProjectByPath(string projectPath)
    {
        Project? resolveProject = _projectResolverService.ResolveProject(projectPath);
        
        if (resolveProject is not null)
        {
            UpdateProject(resolveProject, projectPath);
        }
        else
        {
            _logger.LogWarning("Could not update project because project path was null");
        }
    }

    public void UpdateProject(Project? project)
    {
        UpdateProject(project, null);
    }

    public void UpdateProject(Project? project, string? projectPath = null)
    {
        if (project?.Id is null)
        {
            _logger.LogWarning("Could not update project because project id was null");
            throw new ArgumentNullException(nameof(project.Id));
        }

        if (projectPath is not null && project.Path != projectPath)
        {
            project.Path = projectPath;
            _projectRepository.Update(project);
        }

        _projectFolderService.UpdateProjectFolderMetadata(project);
        _projectStorageService.UpdateStorageInfo(project);

        _projectFileScanningService.ScanProject(project, checkExistingImages: true);
        _projectRepository.GetAllByParentProjectId((int)project.Id!).ForEach(UpdateProject);
    }
}