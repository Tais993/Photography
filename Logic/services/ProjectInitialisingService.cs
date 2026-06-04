using System.Text.RegularExpressions;
using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services;

public class ProjectInitialisingService : IProjectInitialisingService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IImageRepository _imageRepository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IProjectMetadataService _metadataService;
    private readonly IFiles _files;

    public ProjectInitialisingService(IProjectRepository projectRepository, IImageRepository imageRepository,
        ILogger<ProjectService> logger, IProjectMetadataService metadataService, IFiles files)
    {
        _projectRepository = projectRepository;
        _imageRepository = imageRepository;
        _logger = logger;
        _metadataService = metadataService;
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

        _logger.LogInformation("folder name: {FolderName}", pathEnd);


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

            foreach (string subdirectory in subdirectories)
            {
                InitialiseFolder(subdirectory);
            }
        }
    }

    private void InitialiseCollectionFolder(string subdirectory)
    {
        foreach (string directory in _files.GetDirectories(subdirectory))
        {
            InitialiseFolder(directory);
        }

        // TODO
        // This is a collection folder for example all concerts photographed at De Pul
        // For now this has no additional functionality and we will ignore it
    }

    public void InitialiseProjectFolder(string projectDirectory, Match match, Project? parentProject = null)
    {
        string projectInfoPath = _files.Combine(projectDirectory, ProjectInfoFile);

        if (_files.Exists(projectInfoPath))
        {
            _logger.LogInformation(
                $"Project info file found:  name: {_files.ReadAllText(projectInfoPath)}");
            return;
        }
        
        _logger.LogInformation("Initialising project: {1}", projectDirectory);

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
        _files.WriteAllText(project.Id + "", projectInfoPath);
        
        InitialiseProjectSubFolders(projectDirectory, project);
    }

    private void InitialiseProjectSubFolders(string projectDirectory, Project project)
    {
        foreach (string subDirectory in _files.GetDirectories(projectDirectory))
        {
            string pathEnd = _files.GetPathEnd(subDirectory);

            if (SubProjectNameRegex.Match(pathEnd) is { Success: true } subProjectMatch)
            {
                InitialiseProjectFolder(subDirectory, subProjectMatch, project);
            }
            else
            {
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
        foreach (string filePath in _files.GetFiles(projectSubDirectory))
        {
            string fileExtension = _files.GetFileExtension(filePath);
            string fileName = _files.GetFileName(filePath).Replace(fileExtension, "");
            string relativeFilePath = _files.GetRelativePath(projectDirectory, filePath);
            
            Image image = new Image(projectId, fileName, fileExtension, relativeFilePath);

            _imageRepository.Insert(image);
        }
    }

    public void CreateProjectFolder()
    {
        throw new NotImplementedException();
    }

    public void UpdateProjectFolder()
    {
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