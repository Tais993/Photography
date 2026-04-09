using System.Text.RegularExpressions;
using PhotographyNET.database.entities;
using PhotographyNET.database.repositories;
using PhotographyNET.services.interfaces;

namespace PhotographyNET.services;

public class ProjectResolver : IProjectResolver
{
    public static readonly Regex PROJECT_NAME_REGEX = new Regex("(\\d\\d\\d\\d)-(\\d{1,2})-(\\d{1,2})-([^.]*)");

    private readonly ProjectRepository repository;

    public ProjectResolver(ProjectRepository repository)
    {
        this.repository = repository;
    }


    public Project resolveProject(string directory)
    {
        var projectInfoLocation = Path.Combine(directory, "project.info");

        int id = int.Parse(File.ReadAllText(projectInfoLocation));

        return repository.GetById(id) ?? throw new Exception($"no project found with given ID`{id}`");
    }

    public void initialiseExistingFolder(string subdirectory)
    {
        var folderName = Path.GetFileName(subdirectory);
        Console.WriteLine($"folder name: {folderName}");


        if (subdirectory.StartsWith("."))
        {
            Console.WriteLine("it is a collection folder");
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
                    Console.WriteLine("Project info file found: " + File.ReadAllText(projectInfoLocation));
                    return;
                }


                Console.WriteLine("it is not a collection folder");


                var dateOnly = new DateOnly(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value));
                Project project = new Project(null, match.Groups[4].Value, subdirectory, dateOnly);

                project = repository.Insert(project);

                File.WriteAllText(projectInfoLocation, project.Id + "");
            }
        }
    }

    public void initializeImages()
    {

    }
}