using System.Text.RegularExpressions;
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
    public static readonly Regex ProjectNameRegex = new Regex("(\\d\\d\\d\\d)-(\\d{1,2})-(\\d{1,2})-([^.]*)");
    public static readonly Regex SubProjectNameRegex = new Regex("\\.([^.]*)");
    public const string ProjectInfoFile = "project.info";

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



    public int ResolveProjectId(string directory)
    {
        string projectInfoPath = _files.Combine(directory, ProjectInfoFile);

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
            return ResolveProjectId(Directory.GetCurrentDirectory());
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
            return ResolveProject(Directory.GetCurrentDirectory());
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

    /// <summary>
    /// Initialises the given projects directory into the database,
    /// additionally this adds a file to the filesystem to remember the projects ID.
    /// Any images from within the project will also be loaded in with its metadata saved into the database.
    ///
    /// Any collection folders are ignored for now, additionally if a subfolder was given its ignored as its not recognized as a project folder.
    ///
    /// In future versions this method will run recursively, including for any sub/collection folders.
    /// </summary>
    /// <param name="subdirectory"></param>
    public void InitialiseExistingFolder(string subdirectory)
    {
        var pathEnd = _files.GetPathEnd(subdirectory);

        _logger.LogInformation("folder name: {FolderName}", pathEnd);


        if (pathEnd.StartsWith("."))
        {
            InitialiseCollectionFolder(subdirectory);
        }
        else if (ProjectNameRegex.Match(pathEnd) is { Success: true } match)
        {
            InitialiseProjectFolder(subdirectory, match, null);
        }
    }

    private void InitialiseProjectFolder(string projectDirectory, Match match, Project? parentProject)
    {
        string projectInfoLocation = _files.Combine(projectDirectory, ProjectInfoFile);

        _logger.LogInformation("Project info location: {1}", projectInfoLocation);

        if (_files.Exists(projectInfoLocation))
        {
            _logger.LogInformation(
                $"Project info file found:  name: {_files.ReadAllText(projectInfoLocation)}");
            return;
        }

        Project project;

        if (parentProject is null)
        {
            project = ToProject(projectDirectory, match);
        }
        else
        {
            project = ToSubProject(projectDirectory, match, parentProject);
        }
        
        _logger.LogDebug($"Project: {project}", project.ToString());

        project = _projectRepository.Insert(project);
        _logger.LogDebug($"Project AFTER INSERT: {project}", project.ToString());
        _files.WriteAllText(project.Id + "", projectInfoLocation);


        _logger.LogDebug(
            $"Project id: {project.Id}, name: {project.Name}, event_date: {project.EventDate}");
        _logger.LogDebug($"File should be written to {projectInfoLocation}");


        _logger.LogDebug($"Going through all directories now");


        foreach (string subDirectory in _files.GetDirectories(projectDirectory))
        {
            string pathEnd = _files.GetPathEnd(subDirectory);

            if (SubProjectNameRegex.Match(pathEnd) is { Success: true } subProjectMatch)
            {
                _logger.LogDebug($"SubProject: {subDirectory}");
                _logger.LogDebug($"ParentProject: {project}");
                InitialiseProjectFolder(subDirectory, subProjectMatch, project);
            }
            else
            {
                _logger.LogDebug($"Image Folder: {subDirectory}");
                InitializeImages(projectDirectory, subDirectory, project.Id.Value);
            }
        }

        _logger.LogDebug($"All directories initialized");
        _logger.LogInformation($"Successfully initialized project and all images");
    }

    private void InitialiseCollectionFolder(string subdirectory)
    {
        _logger.LogInformation("it is a collection folder");

        foreach (string directory in _files.GetDirectories(subdirectory))
        {
            InitialiseExistingFolder(directory);
        }

        // This is a collection folder for example all concerts photographed at De Pul
        // For now this has no additional functionality and we will ignore it
    }

    private static Project ToProject(string subdirectory, Match match)
    {
        DateOnly dateOnly = new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value));
        Project project = new Project(null, match.Groups[4].Value, subdirectory, dateOnly);
        return project;
    }

    private static Project ToSubProject(string directory, Match match, Project parentProject)
    {
        return new Project(null, match.Groups[1].Value, directory, parentProject.EventDate, parentProject.Id);
    }

    /// <summary>
    /// This method expects a project's subfolder already, and this also
    /// </summary>
    /// <param name="projectDirectory">a subfolder from within a project that contains images</param>
    /// <param name="projectSubDirectory">a subfolder from within a project that contains images</param>
    /// <param name="projectId"></param>
    public void InitializeImages(string projectDirectory, string projectSubDirectory, int projectId)
    {
        foreach (string filePath in _files.GetFiles(projectSubDirectory))
        {
            string fileExtension = _files.GetFileExtension(filePath);
            string fileName = _files.GetFileName(filePath).Replace(fileExtension, "");
            string relativeFilePath = _files.GetRelativePath(projectDirectory, filePath);
            
            Image image = new Image(projectId, fileName, fileExtension, relativeFilePath);

            _imageRepository.Insert(image);
        }
    }
}