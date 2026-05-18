using System.Text.RegularExpressions;
using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
using Microsoft.Extensions.Logging;

namespace Application.services;

public partial class ProjectService : IProjectService
{
    public static readonly Regex ProjectNameRegex = GeneratedProjectNameRegex();
    public const string ProjectInfoFile = "project.info";
    private readonly ILogger<ProjectService> _logger;
    private readonly IImageRepository _imageRepository;
    private readonly IProjectRepository _projectRepository;
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
        string projectInfoLocation = _files.Combine(directory, ProjectInfoFile);

        if (!_files.Exists(projectInfoLocation))
        {
            return 0;
        }

        int id = int.Parse(_files.ReadAllText(projectInfoLocation));

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

    public Project ResolveProject(string directory)
    {
        return _projectRepository.GetById(ResolveProjectId(directory));
    }


    public void InitialiseExistingFolder(string projectDirectory)
    {
        string pathEnd = _files.GetPathEnd(projectDirectory);

        _logger.LogInformation("folder name: {FolderName}", pathEnd);


        if (pathEnd.StartsWith('.'))
        {
            _logger.LogInformation("it is a collection folder");
            foreach (string directory in _files.GetDirectories(projectDirectory)) InitialiseExistingFolder(directory);

            // This is a collection folder for example all concerts photographed at De Pul
            // For now this has no additional functionality and we will ignore it
        }
        else
        {
            Match match = ProjectNameRegex.Match(pathEnd);

            if (match.Success)
            {
                string projectInfoLocation = _files.Combine(projectDirectory, ProjectInfoFile);


                if (_files.Exists(projectInfoLocation))
                {
                    _logger.LogInformation(
                        "Project info file found:  name: {ReadAllText}", _files.ReadAllText(projectInfoLocation));
                    return;
                }

                Project project = ToProject(projectDirectory, match);

                project = _projectRepository.Insert(project);
                _files.WriteAllText(project.Id + "", projectInfoLocation);


                _logger.LogDebug(
                    "Project id: {ProjectId}, name: {ProjectName}, event_date: {ProjectEventDate}", project.Id,
                    project.Name, project.EventDate);
                _logger.LogDebug("File should be written to {ProjectInfoLocation}", projectInfoLocation);


                _logger.LogDebug("Going through all images now");

                foreach (string subDirectory in _files.GetDirectories(projectDirectory))
                {
                    InitializeImages(projectDirectory, subDirectory, project.Id.Value);
                }

                _logger.LogDebug("All images initialized");
                _logger.LogInformation("Successfully initialized project and all images");
            }
            // What to do?
        }
    }

    private static Project ToProject(string subdirectory, Match match)
    {
        DateOnly dateOnly = new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value));
        Project project = new Project(null, match.Groups[4].Value, subdirectory, dateOnly);
        return project;
    }

    /// <summary>
    ///     This method expects a project's subfolder already, and this also
    /// </summary>
    /// <param name="projectDirectory">the project directory</param>
    /// <param name="projectSubDirectory">a subfolder from within a project that contains images</param>
    /// <param name="projectId"></param>
    private void InitializeImages(string projectDirectory, string projectSubDirectory, int projectId)
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

    [GeneratedRegex(@"(\d\d\d\d)-(\d{1,2})-(\d{1,2})-([^.]*)")]
    private static partial Regex GeneratedProjectNameRegex();
}