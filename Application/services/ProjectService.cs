using Application.interfaces;
using Application.services.interfaces;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services;

/// <summary>
/// The project services handles with everything project related.
/// Initializing and resolving the most predominant functions.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IFiles _files;


    public ProjectService(IProjectRepository projectRepository, IFiles files,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _files = files;
        _logger = logger;
    }

    public int ResolveProjectId(string directory, int possibleEmptyProjectId = 0)
    {
        if (possibleEmptyProjectId == 0)
        {
            _logger.LogDebug("Resolving project id for directory: {Directory}", directory);
            string projectInfoPath = _files.Combine(directory, Constants.ProjectInfoFile);

            if (!_files.Exists(projectInfoPath))
            {
                _logger.LogWarning("Project info file was not found for directory: {Directory}", directory);
                return 0;
            }

            int id = int.Parse(_files.ReadAllText(projectInfoPath));

            _logger.LogDebug("Project info file found: id: {Id}", id);

            return id;
        }

        _logger.LogInformation("Project ID was provided by user: {Id}", possibleEmptyProjectId);
        return possibleEmptyProjectId;
    }

    public Project? ResolveProject(string directory, int possibleEmptyProjectId = 0)
    {
        int projectId = ResolveProjectId(directory, possibleEmptyProjectId);

        _logger.LogDebug("Resolving project: {ProjectId}", projectId);

        if (projectId == 0) return null;
        
        return _projectRepository.GetById(projectId);
    }

    public int GetProjectCount()
    {
        _logger.LogDebug("Getting project count");

        int count = _projectRepository.GetProjectCount();

        _logger.LogDebug("Found {Count} projects", count);

        return count;
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