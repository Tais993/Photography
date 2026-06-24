using Application.interfaces.infrastructure;
using Domain.entities;
using Microsoft.Extensions.Logging;
using static Infrastructure.database.repositories.DatabaseMappers;

namespace Infrastructure.database.repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<ProjectRepository> _logger;

    public ProjectRepository(
        ILogger<ProjectRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    public Project? GetById(int id)
    {
        _logger.LogDebug("Getting project by id: {ProjectId}", id);

        return _db.QueryOrDefault("""
                                  SELECT id, name, path, event_date, parent_project_id FROM public.project 
                                  WHERE id = $1
                                  """, MapProject, id);
    }

    public List<Project> GetAll()
    {
        _logger.LogDebug("Getting all projects");

        List<Project> projects = _db.QueryMultiple("""
                                                   SELECT id, name, path, event_date, parent_project_id FROM public.project
                                                   """, MapProject);

        _logger.LogDebug("Found {Count} projects", projects.Count);

        return projects;
    }

    public Project Insert(Project project)
    {
        _logger.LogDebug("Inserting project: {ProjectName}", project.Name);

        Project insertedProject = _db.Query("""
                                            INSERT INTO public.project(name, path, event_date, parent_project_id) 
                                            VALUES ($1, $2, $3, $4)
                                            RETURNING *
                                            """, MapProject, project.Name, project.Path, project.EventDate,
            project.ParentProjectId);

        _logger.LogDebug("Inserted project: {ProjectId}", insertedProject.Id);

        return insertedProject;
    }

    public void Update(Project project)
    {
        if (project.Id is null)
        {
            _logger.LogWarning("Could not update project because project id was null");
            throw new ArgumentException("Project must have an ID", nameof(project));
        }

        _logger.LogDebug("Updating project: {ProjectId}", project.Id);

        _db.Execute("""
                    UPDATE public.project
                    SET name = $1,
                        path = $2,
                        event_date = $3,
                        parent_project_id = $4
                    WHERE id = $5
                    """, project.Name, project.Path, project.EventDate, project.ParentProjectId, project.Id);
    }


    public void DeleteById(int id)
    {
        _logger.LogDebug("Deleting project: {ProjectId}", id);

        _db.Execute("""
                    DELETE FROM public.project 
                    WHERE id = ($1)
                    """, id);
    }

    public List<Project> GetAllByParentProjectId(int parentProjectId)
    {
        _logger.LogDebug("Getting all projects with parent project id: {ParentProjectId}", parentProjectId);

        return _db.QueryMultiple("""
                                 SELECT id, name, path, event_date, parent_project_id
                                 FROM public.project
                                 WHERE parent_project_id = $1
                                 """, MapProject, parentProjectId);
    }

    public int GetProjectCount()
    {
        _logger.LogDebug("Getting project count");

        int count = _db.QueryScalar<int>("""
                                         SELECT COUNT(*) 
                                         FROM public.project
                                         """);

        _logger.LogDebug("Found {Count} projects", count);

        return count;
    }
}