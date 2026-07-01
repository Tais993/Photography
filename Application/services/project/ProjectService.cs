using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.services.project;

/// <summary>
/// The project service handles creating and retrieving projects.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProjectService> _logger;
    private readonly IProjectFolderService _projectFolderService;
    private readonly IProjectInfoFileService _projectInfoFileService;
    private readonly IFiles _files;

    public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger, IFiles files, IProjectFolderService projectFolderService, IConfiguration configuration, IProjectInfoFileService projectInfoFileService)
    {
        _projectRepository = projectRepository;
        _logger = logger;
        _files = files;
        _projectFolderService = projectFolderService;
        _configuration = configuration;
        _projectInfoFileService = projectInfoFileService;
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
        ValidateProjectName(name);

        string? defaultProjectFolder = _configuration.GetValue<string>(Constants.ConfigProjectFolder);

        if (string.IsNullOrWhiteSpace(defaultProjectFolder))
        {
            throw new InvalidOperationException("Default project folder is not set.");
        }
        
        string projectPath = _files.Combine(defaultProjectFolder, ToProjectPath(name, date));

        if (_files.Exists(projectPath))
        {
            throw new InvalidOperationException($"Project folder already exists: {projectPath}");
        }
        
        Project project = new(name.Trim(), projectPath, date);
        project = _projectRepository.Insert(project);

        _projectFolderService.CreateRequiredFolders(project);
        _projectInfoFileService.WriteProjectInfoFile(project);

        return project;
    }

    public Project CreateSubProject(int parentProjectId, string name)
    {
        string projectName = ToSubProjectName(name);
        ValidateProjectName(projectName);

        Project? parentProject = GetProjectById(parentProjectId);

        if (parentProject is null)
        {
            throw new InvalidOperationException($"Parent project with id {parentProjectId} was not found.");
        }
        
        string projectPath = _files.Combine(parentProject.Path, ToSubProjectPath(projectName));

        if (_files.Exists(projectPath))
        {
            throw new InvalidOperationException($"Subproject folder already exists: {projectPath}");
        }
        
        Project project = new(projectName, projectPath, parentProject.EventDate, parentProjectId);
        project = _projectRepository.Insert(project);

        _projectFolderService.CreateRequiredFolders(project);
        _projectInfoFileService.WriteProjectInfoFile(project);

        return project;
    }

    private void ValidateProjectName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name cannot be empty.", nameof(name));
        }
    }

    private string ToProjectPath(string name, DateOnly date)
    {
        return date.ToString("yyyy-MM-dd") + "-" + name.Trim();
    }

    private string ToSubProjectName(string name)
    {
        return name.Trim();;
    }

    private string ToSubProjectPath(string name)
    {
        return "." + ToSubProjectName(name);
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