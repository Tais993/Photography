using System.Text.RegularExpressions;
using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services.project;

public class ProjectInitialisingService : IProjectInitialisingService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectInfoFileService _projectInfoFileService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProjectInitialisingService> _logger;
    private readonly IFiles _files;
    private readonly ICollectionMetadataService _collectionMetadataService;

    public ProjectInitialisingService(IProjectRepository projectRepository, IImageRepository imageRepository,
        IProjectMetadataService projectMetadataService, IProjectInfoFileService projectInfoFileService,
        IConfiguration configuration, ILogger<ProjectInitialisingService> logger,
        IFiles files, ICollectionMetadataService collectionMetadataService)
    {
        _projectRepository = projectRepository;
        _imageRepository = imageRepository;
        _projectMetadataService = projectMetadataService;
        _projectInfoFileService = projectInfoFileService;
        _configuration = configuration;
        _logger = logger;
        _files = files;
        _collectionMetadataService = collectionMetadataService;
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
        InitialiseFolder(folderDirectory, null);
    }

    private void InitialiseFolder(string folderDirectory, CollectionMetadataConfiguration? collectionMetadataConfiguration)
    {
        string pathEnd = _files.GetPathEnd(folderDirectory);
        _logger.LogInformation("Initialising folder: {FolderName}", pathEnd);

        if (pathEnd.StartsWith("."))
        {
            InitialiseCollectionFolder(folderDirectory);
        }
        else if (ProjectNameRegex.Match(pathEnd) is { Success: true } match)
        {
            InitialiseProjectFolder(folderDirectory, match, null, collectionMetadataConfiguration);
        }
        else
        {
            string[] subdirectories = _files.GetDirectories(folderDirectory);
            _logger.LogInformation("Folder is not a project folder, initialising {Count} subdirectories: {FolderName}", subdirectories.Length, pathEnd);

            foreach (string subdirectory in subdirectories)
            {
                InitialiseFolder(subdirectory, collectionMetadataConfiguration);
            }
        }
    }

    private void InitialiseCollectionFolder(string subdirectory)
    {
        _logger.LogInformation("Initialising collection folder: {FolderName}", subdirectory);

        CollectionMetadataConfiguration? collectionMetadataConfiguration = _collectionMetadataService.GetCollectionMetadataConfiguration(subdirectory);

        if (collectionMetadataConfiguration is null)
        {
            _logger.LogDebug("No collection metadata configuration found for collection folder: {FolderName}", subdirectory);
        }
        else
        {
            _logger.LogDebug("Found collection metadata configuration: {MetadataKey}", collectionMetadataConfiguration.MetadataKey);
        }

        string[] directories = _files.GetDirectories(subdirectory);
        _logger.LogInformation("Initialising {Count} folders in collection folder: {FolderName}", directories.Length, subdirectory);

        foreach (string directory in directories)
        {
            InitialiseFolder(directory, collectionMetadataConfiguration);
        }
    }

    public void InitialiseProjectFolder(string projectDirectory, Match match, Project? parentProject = null)
    {
        InitialiseProjectFolder(projectDirectory, match, parentProject, null);
    }

    private void InitialiseProjectFolder(string projectDirectory, Match match, Project? parentProject, 
        CollectionMetadataConfiguration? collectionMetadataConfiguration)
    {
        _logger.LogInformation("Initialising project folder: {FolderName}", projectDirectory);
        string projectInfoPath = _files.Combine(projectDirectory, ProjectInfoFile);

        if (_projectInfoFileService.HasProjectInfoFile(projectDirectory))
        {
            int? existingProjectId = _projectInfoFileService.ReadProjectId(projectDirectory);
            _logger.LogInformation("Project already initialised, existing project info file found, id: {ProjectId}", existingProjectId);
            return;
        }

        Project project = parentProject is null
            ? ToProject(projectDirectory, match)
            : ToSubProject(projectDirectory, match, parentProject);

        project = _projectRepository.Insert(project);
        _logger.LogInformation("Created project: {ProjectId}, name: {ProjectName}", project.Id, project.Name);

        _projectInfoFileService.WriteProjectInfoFile(project);

        InitialiseProjectFolderMetadata(projectDirectory, project);
        InitialiseProjectCollectionMetadata(project, collectionMetadataConfiguration);
        InitialiseProjectSubFolders(projectDirectory, project, collectionMetadataConfiguration);
    }

    private void InitialiseProjectFolderMetadata(string projectDirectory, Project project)
    {
        if (project.Id is null)
        {
            _logger.LogWarning("Could not initialise project folder metadata because project id was null");
            throw new ArgumentNullException(nameof(project.Id));
        }

        Dictionary<string, string[]> folderNamesByRole = GetConfiguredFolderNames();

        if (folderNamesByRole.Count == 0)
        {
            _logger.LogDebug("No configured folder names found for project folder mapping");
            return;
        }

        string[] folderNames = _files.GetDirectories(projectDirectory)
            .Select(directory => _files.GetPathEnd(directory))
            .ToArray();

        foreach ((string folderRole, string[] possibleFolderNames) in folderNamesByRole)
        {
            string[] matchingFolderNames = CompareFolderNames(folderNames, possibleFolderNames);
            string metadataKey = ToFolderMetadataKey(folderRole);

            if (matchingFolderNames.Length == 0)
            {
                _logger.LogDebug(
                    "No folder found for role {FolderRole} in project {ProjectId}",
                    folderRole,
                    project.Id);

                continue;
            }

            if (matchingFolderNames.Length > 1)
            {
                _logger.LogWarning(
                    "Multiple folders found for role {FolderRole} in project {ProjectId}: {FolderNames}",
                    folderRole,
                    project.Id,
                    string.Join(", ", matchingFolderNames));

                continue;
            }

            _projectMetadataService.AddMetadataToProject(
                project.Id.Value,
                metadataKey,
                matchingFolderNames[0]);

            _logger.LogInformation(
                "Mapped project folder role {FolderRole} to folder {FolderName} for project {ProjectId}",
                folderRole,
                matchingFolderNames[0],
                project.Id);
        }
    }

    private void InitialiseProjectCollectionMetadata(Project project, CollectionMetadataConfiguration? collectionMetadataConfiguration)
    {
        if (collectionMetadataConfiguration is null)
        {
            return;
        }

        if (project.Id is null)
        {
            _logger.LogWarning("Could not initialise project collection metadata because project id was null");
            throw new ArgumentNullException(nameof(project.Id));
        }

        if (string.IsNullOrWhiteSpace(collectionMetadataConfiguration.MetadataKey))
        {
            _logger.LogWarning("Could not initialise project collection metadata because metadata key was empty");
            return;
        }

        _projectMetadataService.AddMetadataToProject(
            project.Id.Value,
            collectionMetadataConfiguration.MetadataKey,
            collectionMetadataConfiguration.MetadataValue);
        
        _logger.LogInformation("Added collection metadata {MetadataKey} to project {ProjectId}", collectionMetadataConfiguration.MetadataKey, project.Id);
    }

    private Dictionary<string, string[]> GetConfiguredFolderNames()
    {
        return _configuration
            .GetSection(FolderNamesConfigKey)
            .GetChildren()
            .ToDictionary(
                section => section.Key,
                section => section.GetChildren()
                    .Select(child => child.Value)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!)
                    .ToArray());
    }

    public static string[] CompareFolderNames(string[] folderNames, string[] possibleFolderNames)
    {
        return folderNames
            .Where(folderName => possibleFolderNames.Contains(folderName, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string ToFolderMetadataKey(string folderRole)
    {
        return FolderMetadataKeyPrefix + folderRole.ToLowerInvariant();
    }

    private void InitialiseProjectSubFolders(string projectDirectory, Project project, CollectionMetadataConfiguration? collectionMetadataConfiguration)
    {
        string[] subDirectories = _files.GetDirectories(projectDirectory);
        _logger.LogDebug("Initialising {Count} subdirectories for project: {ProjectId}", subDirectories.Length, project.Id);

        foreach (string subDirectory in subDirectories)
        {
            string pathEnd = _files.GetPathEnd(subDirectory);

            if (SubProjectNameRegex.Match(pathEnd) is { Success: true } subProjectMatch)
            {
                _logger.LogDebug("Initialising project sub-project: {FolderName}", pathEnd);
                InitialiseProjectFolder(subDirectory, subProjectMatch, project, collectionMetadataConfiguration);
            }
            else
            {
                _logger.LogDebug("Initialising project sub-folder's images: {FolderName}", pathEnd);
                InitializeImages(projectDirectory, subDirectory, project.Id!.Value);
            }
        }
    }


    /// <summary>
    /// This method expects a project's subfolder already.
    /// </summary>
    /// <param name="projectDirectory">The project root directory.</param>
    /// <param name="projectSubDirectory">A subfolder from within a project that contains images.</param>
    /// <param name="projectId">The project id.</param>
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

            _imageRepository.Insert(image);
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
        DateOnly dateOnly = new DateOnly(int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));

        return new Project(match.Groups[4].Value, subdirectory, dateOnly);
    }

    private static Project ToSubProject(string directory, Match match, Project parentProject)
    {
        return new Project(match.Groups[1].Value, directory, parentProject.EventDate, parentProject.Id);
    }
}