using System.Text.RegularExpressions;
using Application.services.interfaces;
using Domain.entities;
using Infrastructure.database.repositories;
using Infrastructure.filesystem;
using Microsoft.Extensions.Logging;

namespace Application.services;

public class ProjectService : IProjectResolver
{
    public static readonly Regex PROJECT_NAME_REGEX = new Regex("(\\d\\d\\d\\d)-(\\d{1,2})-(\\d{1,2})-([^.]*)");

    private readonly ProjectRepository _repository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IFiles _files;


    public ProjectService(ProjectRepository repository, IFiles files, ILogger<ProjectService> logger)
    {
        _repository = repository;
        _files = files;
        _logger = logger;
    }


    public Project resolveProject(string directory)
    {
        var projectInfoLocation = _files.PathCombine(directory, "project.info");

        int id = int.Parse(_files.ReadFile(projectInfoLocation));

        _logger.LogInformation($"project info file found:  id: {id}");

        return _repository.GetByKey(id) ?? throw new Exception($"no project found with given ID`{id}`");
    }

    public void initialiseExistingFolder(string subdirectory)
    {
        var folderName = Path.GetFileName(subdirectory);
        _logger.LogInformation($"folder name: {folderName}");


        if (subdirectory.StartsWith("."))
        {
            _logger.LogInformation("it is a collection folder");
            // This is a collection folder for example all concerts photographed at De Pul
            // For now this has no additional functionality and we will keep this empty
        }
        else
        {
            var match = PROJECT_NAME_REGEX.Match(folderName);

            if (match.Success)
            {

                string projectInfoLocation = _files.PathCombine(subdirectory, "project.info");


                if (_files.PathExists(projectInfoLocation))
                {
                    _logger.LogInformation($"Project info file found:  name: {_files.ReadFile(projectInfoLocation)}");
                    return;
                }


                var dateOnly = new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value));
                Project project = new Project(null, match.Groups[4].Value, subdirectory, dateOnly);

                project = _repository.Insert(project);

                _logger.LogInformation($"Project id: {project.Id}, name: {project.Name}, event_date: {project.EventDate}");

                _files.WriteFile(project.Id + "", projectInfoLocation);

                _logger.LogInformation($"File should be written to {projectInfoLocation}");

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