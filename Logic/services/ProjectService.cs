using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
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
        ILogger<ProjectService> logger, FolderNameOptions options, IProjectMetadataService metadataService)
    {
        _projectRepository = projectRepository;
        _imageRepository = imageRepository;
        _files = files;
        _logger = logger;
    }



    public int ResolveProjectId(string directory)
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

    public int ResolveProjectId(string directory, int possibleEmptyProjectId)
    {
        if (possibleEmptyProjectId == 0)
        {
            return ResolveProjectId(directory);
        }

        return possibleEmptyProjectId;
    }

    public Project? ResolveProject(string directory)
    {
        return _projectRepository.GetById(ResolveProjectId(directory));
    }

    public Project? ResolveProject(string directory, int possibleEmptyProjectId)
    {
        if (possibleEmptyProjectId == 0)
        {
            return ResolveProject(directory);
        }

        return _projectRepository.GetById(possibleEmptyProjectId);
    }

    public Image GetImageById(int imageId)
    {
        return _imageRepository.GetById(imageId);
    }

    public int GetProjectImageCount(int projectId)
    {
        return _imageRepository.GetProjectImageCount(projectId);
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