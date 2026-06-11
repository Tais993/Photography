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
    private readonly IImageRepository _imageRepository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IFiles _files;


    public ProjectService(IProjectRepository projectRepository, IImageRepository imageRepository, IFiles files,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _imageRepository = imageRepository;
        _files = files;
        _logger = logger;
    }



    public int ResolveProjectId(string directory, int possibleEmptyProjectId = 0)
    {
        if (possibleEmptyProjectId == 0)
        {
            string projectInfoPath = _files.Combine(directory, Constants.ProjectInfoFile);

            if (!_files.Exists(projectInfoPath))
            {
                return 0;
            }

            int id = int.Parse(_files.ReadAllText(projectInfoPath));

            _logger.LogInformation("project info file found:  id: {Id}", id);

            return id;
        }

        return possibleEmptyProjectId;
    }

    public Project? ResolveProject(string directory, int possibleEmptyProjectId = 0)
    {
        if (possibleEmptyProjectId == 0)
        {
            return _projectRepository.GetById(ResolveProjectId(directory));
        }

        return _projectRepository.GetById(possibleEmptyProjectId);
    }

    public int GetProjectCount()
    {
        return _projectRepository.GetProjectCount();
    }

    public Project GetProjectById(int projectId)
    {
        return _projectRepository.GetById(projectId);
    }

    public List<Project> GetAllProjects()
    {
        return _projectRepository.GetAll();
    }
}