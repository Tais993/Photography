using System.Text.RegularExpressions;
using PhotographyNET.database.entities;
using PhotographyNET.database.repositories;
using PhotographyNET.services.interfaces;

namespace PhotographyNET.services;

public class ProjectResolver : IProjectResolver
{
    public static readonly Regex PROJECT_NAME_REGEX = new Regex("(\\d\\d\\d\\d)-(\\d{1,2})-(\\d{1,2})-([^.]*)");

    private readonly ProjectRepository _repository;
    private readonly ILogger<ProjectResolver> _logger;


    public ProjectResolver(ProjectRepository repository, ILogger<ProjectResolver> logger)
    {
        _repository = repository;
        _logger = logger;
    }


    public Project resolveProject(string directory)
    {
        var projectInfoLocation = Path.Combine(directory, "project.info");

        int id = int.Parse(File.ReadAllText(projectInfoLocation));

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
                var projectInfoLocation = Path.Combine(subdirectory, "project.info");
                if (Path.Exists(projectInfoLocation))
                {
                    _logger.LogInformation($"Project info file found:  name: {File.ReadAllText(projectInfoLocation)}");
                    return;
                }



                var dateOnly = new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value));
                Project project = new Project(null, match.Groups[4].Value, subdirectory, dateOnly);

                project = _repository.Insert(project);

                _logger.LogInformation($"Project id: {project.Id}, name: {project.Name}, event_date: {project.EventDate}");


                File.WriteAllText(projectInfoLocation, project.Id + "");

                _logger.LogInformation($"File should be written to {projectInfoLocation}");

            }
        }
    }

    public void initializeImages()
    {
    }
}