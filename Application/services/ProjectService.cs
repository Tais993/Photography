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
public class ProjectService : IProjectResolver
{
    public static readonly Regex ProjectNameRegex = new Regex("(\\d\\d\\d\\d)-(\\d{1,2})-(\\d{1,2})-([^.]*)");
    public const string ProjectInfoFile = "project.info";

    private readonly IProjectRepository _repository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IFiles _files;


    public ProjectService(IProjectRepository repository, IFiles files, ILogger<ProjectService> logger)
    {
        _repository = repository;
        _files = files;
        _logger = logger;
    }


    /// <summary>
    /// Based on a given directory, resolves any projects that can be found within the main, and or its parent folder.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Project resolveProject(string directory)
    {
        var projectInfoLocation = _files.Combine(directory, ProjectInfoFile);

        if (!_files.Exists(projectInfoLocation))
        {
            throw new InvalidOperationException($"No {ProjectInfoFile} found");
        }

        int id = int.Parse(_files.ReadAllText(projectInfoLocation));

        _logger.LogInformation($"project info file found:  id: {id}");

        return _repository.GetByKey(id);
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
    public void initialiseExistingFolder(string subdirectory)
    {
        var pathEnd = _files.GetPathEnd(subdirectory);

        _logger.LogInformation("folder name: {FolderName}", pathEnd);


        if (pathEnd.StartsWith("."))
        {
            _logger.LogInformation("it is a collection folder");
            // This is a collection folder for example all concerts photographed at De Pul
            // For now this has no additional functionality and we will ignore it
        }
        else
        {
            var match = ProjectNameRegex.Match(pathEnd);

            if (match.Success)
            {
                string projectInfoLocation = _files.Combine(subdirectory, ProjectInfoFile);


                if (_files.Exists(projectInfoLocation))
                {
                    _logger.LogInformation(
                        $"Project info file found:  name: {_files.ReadAllText(projectInfoLocation)}");
                    return;
                }


                var dateOnly = new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value));
                Project project = new Project(null, match.Groups[4].Value, subdirectory, dateOnly);

                project = _repository.Insert(project);

                _logger.LogInformation(
                    $"Project id: {project.Id}, name: {project.Name}, event_date: {project.EventDate}");

                _files.WriteAllText(project.Id + "", projectInfoLocation);

                _logger.LogInformation($"File should be written to {projectInfoLocation}");
            }
            else
            {
                // What to do?
            }
        }
    }

    /// <summary>
    /// This method expects a project's subfolder already, and
    /// </summary>
    /// <param name="projectDirectory">a subfolder from within a project that contains images</param>
    public void initializeImages(string projectDirectory)
    {

    }
}