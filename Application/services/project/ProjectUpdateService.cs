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
    
    private readonly IProjectScanningService _projectScanningService;
    private readonly IProjectStorageService _projectStorageService;
    private readonly IProjectFolderService _projectFolderService;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectResolverService _projectResolverService;
    private readonly ILogger<ProjectUpdateService> _logger;

    public ProjectUpdateService(IProjectScanningService projectScanningService, IProjectStorageService projectStorageService,
        IProjectRepository projectRepository, ILogger<ProjectUpdateService> logger, IProjectFolderService projectFolderService, IProjectResolverService projectResolverService)
    {
        _projectScanningService = projectScanningService;
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
            UpdateProject(resolveProject);
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
        } 
        
        _projectFolderService.UpdateProjectFolderMetadata(project);
        _projectStorageService.UpdateStorageInfo(project);

        _projectScanningService.ScanProject(project);
        _projectRepository.GetAllByParentProjectId((int)project.Id!).ForEach(UpdateProject);
    }
}