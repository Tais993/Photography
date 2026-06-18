using System.Text.RegularExpressions;
using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services;

public class ProjectInitialisingService : IProjectInitialisingService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IImageRepository _imageRepository;
    private readonly ILogger<ProjectInitialisingService> _logger;
    private readonly IFiles _files;

    public ProjectInitialisingService(
        IProjectRepository projectRepository,
        IImageRepository imageRepository,
        ILogger<ProjectInitialisingService> logger,
        IFiles files)
    {
        _projectRepository = projectRepository;
        _imageRepository = imageRepository;
        _logger = logger;
        _files = files;
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
    /// <param name="folderDirectory"></param>
    public void InitialiseFolder(string folderDirectory)
    {
        string pathEnd = _files.GetPathEnd(folderDirectory);
        _logger.LogInformation("Initialising folder: {FolderName}", pathEnd);
        

        if (pathEnd.StartsWith("."))
        {
            InitialiseCollectionFolder(folderDirectory);
        }
        else if (ProjectNameRegex.Match(pathEnd) is { Success: true } match)
        {
            InitialiseProjectFolder(folderDirectory, match);
        }
        else
        {
            string[] subdirectories = _files.GetDirectories(folderDirectory);
            _logger.LogInformation("Folder is not a project folder, initialising {Count} subdirectories: {FolderName}", subdirectories.Length, pathEnd);

            foreach (string subdirectory in subdirectories)
            {
                InitialiseFolder(subdirectory);
            }
        }
    }

    private void InitialiseCollectionFolder(string subdirectory)
    {
        _logger.LogInformation("Initialising collection folder: {FolderName}", subdirectory);

        string[] directories = _files.GetDirectories(subdirectory);
        _logger.LogInformation("Initialising {Count} folders in collection folder: {FolderName}", directories.Length, subdirectory);

        foreach (string directory in directories)
        {
            InitialiseFolder(directory);
        }

        // TODO
        // This is a collection folder for example all concerts photographed at De Pul
        // For now this has no additional functionality and we will ignore it
    }

    public void InitialiseProjectFolder(string projectDirectory, Match match, Project? parentProject = null)
    {
        _logger.LogInformation("Initialising project folder: {FolderName}", projectDirectory);
        string projectInfoPath = _files.Combine(projectDirectory, ProjectInfoFile);

        if (_files.Exists(projectInfoPath))
        {
            string existingProjectId = _files.ReadAllText(projectInfoPath);
            _logger.LogInformation("Project already initialised, existing project info file found, id: {ProjectId}", existingProjectId);
            return;
        }

        _logger.LogInformation("Initialising project: {ProjectDirectory}", projectDirectory);

        Project project;

        if (parentProject is null)
        {
            project = ToProject(projectDirectory, match);
        }
        else
        {
            project = ToSubProject(projectDirectory, match, parentProject);
        }

        project = _projectRepository.Insert(project);
        _logger.LogInformation("Created project: {ProjectId}, name: {ProjectName}", project.Id, project.Name);

        _files.WriteAllText(project.Id + "", projectInfoPath);
        _logger.LogInformation("Wrote project info file: {ProjectInfoPath}", projectInfoPath);

        InitialiseProjectSubFolders(projectDirectory, project);
    }

    private void InitialiseProjectSubFolders(string projectDirectory, Project project)
    {
        string[] subDirectories = _files.GetDirectories(projectDirectory);
        _logger.LogDebug("Initialising {Count} subdirectories for project: {ProjectId}", subDirectories.Length, project.Id);

        foreach (string subDirectory in subDirectories)
        {
            string pathEnd = _files.GetPathEnd(subDirectory);
            
            if (SubProjectNameRegex.Match(pathEnd) is { Success: true } subProjectMatch)
            {
                _logger.LogDebug("Initialising project sub-project: {FolderName}", pathEnd);
                InitialiseProjectFolder(subDirectory, subProjectMatch, project);
            }
            else
            {
                _logger.LogDebug("Initialising project sub-folder's images: {FolderName}", pathEnd);
                InitializeImages(projectDirectory, subDirectory, project.Id.Value);
            }
        }
    }


    /// <summary>
    /// This method expects a project's subfolder already, and this also
    /// </summary>
    /// <param name="projectDirectory">a subfolder from within a project that contains images</param>
    /// <param name="projectSubDirectory">a subfolder from within a project that contains images</param>
    /// <param name="projectId"></param>
    public void InitializeImages(string projectDirectory, string projectSubDirectory, int projectId)
    {
        string[] files = _files.GetFiles(projectSubDirectory);
        _logger.LogDebug("Initialising {Count} images for project: {ProjectId}, folder: {FolderName}", files.Length, projectId, projectSubDirectory);

        foreach (string filePath in files)
        {
            string fileExtension = _files.GetFileExtension(filePath);
            string fileName = _files.GetFileName(filePath).Replace(fileExtension, "");
            string relativeFilePath = _files.GetRelativePath(projectDirectory, filePath);

            Image image = new Image(projectId, fileName, fileExtension, relativeFilePath);

            Image insertedImage = _imageRepository.Insert(image);
            _logger.LogTrace("Inserted image file: {RelativeFilePath}", relativeFilePath);
        }

        _logger.LogDebug("Finished initialising {Count} images for project: {ProjectId}, folder: {FolderName}", files.Length, projectId, projectSubDirectory);
    }

    public void CreateProjectFolder()
    {
        _logger.LogWarning("CreateProjectFolder is not implemented");
        throw new NotImplementedException();
    }

    public void UpdateProjectFolder()
    {
        _logger.LogWarning("UpdateProjectFolder is not implemented");
        throw new NotImplementedException();
    }





    private static Project ToProject(string subdirectory, Match match)
    {
        DateOnly dateOnly = new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value));
        return new Project(null, match.Groups[4].Value, subdirectory, dateOnly);
    }

    private static Project ToSubProject(string directory, Match match, Project parentProject)
    {
        return new Project(null, match.Groups[1].Value, directory, parentProject.EventDate, parentProject.Id);
    }
}