using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services.project;

/// <summary>
/// The project services handles with everything project related.
/// Initializing and resolving the most predominant functions.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;


    public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
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