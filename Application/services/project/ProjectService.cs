using Application.interfaces.infrastructure;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.services.project;

/// <summary>
/// The project services handles with everything project related.
/// Initializing and resolving the most predominant functions.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProjectService> _logger;
    private readonly IProjectFolderService _projectFolderService;
    private readonly IFiles _files;


    public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger, IFiles files, IProjectFolderService projectFolderService, IConfiguration configuration)
    {
        _projectRepository = projectRepository;
        _logger = logger;
        _files = files;
        _projectFolderService = projectFolderService;
        _configuration = configuration;
    }
    
    public int GetProjectCount()
    {
        _logger.LogDebug("Getting project count");

        int count = _projectRepository.GetProjectCount();

        _logger.LogDebug("Found {Count} projects", count);

        return count;
    }

    public Project CreateProject(string name, DateOnly date)
    {
        string? defaultProjectFolder = _configuration.GetValue<string>(Constants.ConfigProjectFolder);

        if (defaultProjectFolder is null)
        {
            throw new InvalidOperationException("Default project folder is not set.");
        }
        
        string projectPath = _files.Combine(defaultProjectFolder, ToProjectPath(name, date));
        
        Project project = new(name, projectPath, date);
        project = _projectRepository.Insert(project);

        _projectFolderService.CreateRequiredFolders(project);

        return project;
    }

    private string ToProjectPath(string name, DateOnly date)
    {
        return date.ToString("yyyy-MM-dd") + "-" + name;
    }

    public Project? GetProjectById(int projectId)
    {
        _logger.LogDebug("Getting project by id: {ProjectId}", projectId);
        return _projectRepository.GetById(projectId);
    }

    public List<Project> GetAllProjects()
    {
        _logger.LogDebug("Getting all projects");

        List<Project> projects = _projectRepository.GetAll();

        _logger.LogDebug("Found {Count} projects", projects.Count);

        return projects;
    }
}