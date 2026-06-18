using System.Text;
using Application.interfaces.infrastructure;
using Domain.entities;
using Domain.entities.search;
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

    public int GetProjectCount()
    {
        _logger.LogDebug("Getting project count");

        int count = _db.Query("""
                              SELECT COUNT(*) 
                              FROM public.project
                              """, _db.MapToInt);

        _logger.LogDebug("Found {Count} projects", count);

        return count;
    }

    public List<Project> SearchProjects(ProjectSearchSettings projectSearchSettings)
    {
        _logger.LogDebug("Searching projects");

        StringBuilder sqlBuilder = new();
        List<string> whereClauses = [];
        List<object?> parameters = [];

        sqlBuilder.Append("""
                          SELECT id, name, path, event_date, parent_project_id
                          FROM public.project
                          """);

        if (projectSearchSettings.ProjectId is not null)
        {
            parameters.Add(projectSearchSettings.ProjectId);
            whereClauses.Add($"id = ${parameters.Count}::int");
        }

        if (projectSearchSettings.ParentProjectId is not null)
        {
            parameters.Add(projectSearchSettings.ParentProjectId);
            whereClauses.Add($"parent_project_id = ${parameters.Count}::int");
        }

        if (!string.IsNullOrWhiteSpace(projectSearchSettings.ProjectName))
        {
            parameters.Add(projectSearchSettings.ProjectName);
            whereClauses.Add($"name ILIKE ('%' || ${parameters.Count}::text || '%')");
        }

        if (!string.IsNullOrWhiteSpace(projectSearchSettings.ProjectPath))
        {
            parameters.Add(projectSearchSettings.ProjectPath);
            whereClauses.Add($"path ILIKE ('%' || ${parameters.Count}::text || '%')");
        }

        if (projectSearchSettings.EventDate is not null)
        {
            parameters.Add(projectSearchSettings.EventDate);
            whereClauses.Add($"event_date = ${parameters.Count}::date");
        }

        if (whereClauses.Count > 0)
        {
            sqlBuilder.AppendLine("\nWHERE " + string.Join("\n  AND ", whereClauses));
        }

        sqlBuilder.AppendLine("\nORDER BY event_date DESC, name");

        List<Project> projects = _db.QueryMultiple(sqlBuilder.ToString(),
            MapProject, parameters.ToArray());

        _logger.LogDebug("Project search returned {Count} projects", projects.Count);

        return projects;
    }
}