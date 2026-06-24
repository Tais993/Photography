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
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IProjectScanningService _projectScanningService;
    private readonly IProjectInfoFileService _projectInfoFileService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProjectInitialisingService> _logger;
    private readonly IFiles _files;
    private readonly ICollectionMetadataService _collectionMetadataService;

    public ProjectInitialisingService(IProjectRepository projectRepository, IProjectMetadataService projectMetadataService, 
        IProjectInfoFileService projectInfoFileService, IConfiguration configuration, ILogger<ProjectInitialisingService> logger,
        IFiles files, ICollectionMetadataService collectionMetadataService, IProjectScanningService projectScanningService)
    {
        _projectRepository = projectRepository;
        _projectMetadataService = projectMetadataService;
        _projectInfoFileService = projectInfoFileService;
        _configuration = configuration;
        _logger = logger;
        _files = files;
        _collectionMetadataService = collectionMetadataService;
        _projectScanningService = projectScanningService;
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
            Project? project = InitialiseProjectFolder(folderDirectory, match, null, collectionMetadataConfiguration);

            if (project is not null)
            {
                _projectScanningService.ScanProject(project);
            }
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
        Project? project = InitialiseProjectFolder(projectDirectory, match, parentProject, null);

        if (project is not null)
        {
            _projectScanningService.ScanProject(project);
        }
    }

    private Project? InitialiseProjectFolder(string projectDirectory, Match match, Project? parentProject, CollectionMetadataConfiguration? collectionMetadataConfiguration)
    {
        _logger.LogInformation("Initialising project folder: {FolderName}", projectDirectory);

        Project? project;

        if (_projectInfoFileService.HasProjectInfoFile(projectDirectory))
        {
            project = GetExistingProject(projectDirectory);

            if (project is null)
            {
                return null;
            }

            _logger.LogInformation("Project already initialised, existing project info file found, id: {ProjectId}", project.Id);
        }
        else
        {
            project = parentProject is null
                ? ToProject(projectDirectory, match)
                : ToSubProject(projectDirectory, match, parentProject);

            project = _projectRepository.Insert(project);
            _logger.LogInformation("Created project: {ProjectId}, name: {ProjectName}", project.Id, project.Name);

            _projectInfoFileService.WriteProjectInfoFile(project);

            InitialiseProjectFolderMetadata(projectDirectory, project);
            InitialiseProjectCollectionMetadata(project, collectionMetadataConfiguration);
        }

        InitialiseProjectSubFolders(projectDirectory, project, collectionMetadataConfiguration);

        return project;
    }

    private Project? GetExistingProject(string projectDirectory)
    {
        int? existingProjectId = _projectInfoFileService.ReadProjectId(projectDirectory);

        if (existingProjectId is null)
        {
            _logger.LogWarning("Project info file did not contain a valid project id: {ProjectDirectory}", projectDirectory);
            return null;
        }

        Project? project = _projectRepository.GetById(existingProjectId.Value);

        if (project is null)
        {
            _logger.LogWarning("Project info file points to a project that does not exist: {ProjectId}", existingProjectId);
            return null;
        }

        return project;
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
                _logger.LogDebug("No folder found for role {FolderRole} in project {ProjectId}", folderRole, project.Id);
                continue;
            }

            if (matchingFolderNames.Length > 1)
            {
                _logger.LogWarning("Multiple folders found for role {FolderRole} in project {ProjectId}: {FolderNames}", folderRole, project.Id, string.Join(", ", matchingFolderNames));
                continue;
            }

            _projectMetadataService.AddMetadataToProject(
                project.Id.Value,
                metadataKey,
                matchingFolderNames[0]);

            _logger.LogInformation("Mapped project folder role {FolderRole} to folder {FolderName} for project {ProjectId}", folderRole, matchingFolderNames[0], project.Id);
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

            if (SubProjectNameRegex.Match(pathEnd) is not { Success: true } subProjectMatch)
            {
                continue;
            }

            _logger.LogDebug("Initialising project sub-project: {FolderName}", pathEnd);

            InitialiseProjectFolder(subDirectory, subProjectMatch, project, collectionMetadataConfiguration);
        }
    }

    private static Project ToProject(string subdirectory, Match match)
    {
        DateOnly dateOnly = new DateOnly(
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value));

        return new Project(match.Groups[4].Value, subdirectory, dateOnly);
    }

    private static Project ToSubProject(string directory, Match match, Project parentProject)
    {
        return new Project(match.Groups[1].Value, directory, parentProject.EventDate, parentProject.Id);
    }
}