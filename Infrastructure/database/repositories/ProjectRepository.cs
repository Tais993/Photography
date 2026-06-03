using System.Text;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly RepositoryHelper _db;
    private readonly ILogger<ProjectRepository> _logger;

    public ProjectRepository(NpgsqlDataSource dataSource,
        ILogger<ProjectRepository> logger,
        RepositoryHelper db)
    {
        _logger = logger;
        _db = db;
    }

    public Project GetById(int id)
    {
        _logger.LogInformation("GetByKey, with params: {Id}", id);
        return _db.Query("""
                         SELECT id, name, path, event_date, parent_project_id FROM public.project 
                         WHERE id = $1
                         """, MapProject, id);
    }

    public List<Project> GetAll()
    {
        return _db.QueryMultiple("""
                                 SELECT id, name, path, event_date, parent_project_id FROM public.project
                                 """, MapProject);
    }

    public Project Insert(Project project)
    {
        return _db.Query("""
                         INSERT INTO public.project(name, path, event_date, parent_project_id) 
                         VALUES ($1, $2, $3, $4)
                         RETURNING *
                         """, MapProject, project.Name, project.Path, project.EventDate, project.ParentProjectId);
    }

    public void Update(Project project)
    {
        if (project.Id is null) throw new ArgumentException("Project must have an ID", nameof(project));

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
        _db.Execute("""
                    DELETE FROM public.project 
                    WHERE id = ($1)
                    """, id);
    }

    public int GetProjectCount()
    {
        return _db.Query("""
                         SELECT COUNT(*) 
                         FROM public.project
                         """, _db.MapToInt);
    }

    public List<Project> SearchProjects(ProjectSearchSettings projectSearchSettings)
    {

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
        
        return _db.QueryMultiple(sqlBuilder.ToString(),
            MapProject, parameters.ToArray());
    }

    private static Project MapProject(NpgsqlDataReader reader)
    {
        if (!reader.HasRows) return null!;
        
        int? parentProjectId = reader["parent_project_id"] == DBNull.Value
            ? null
            : (int)reader["parent_project_id"];


        return new Project(
            (int)reader["id"],
            (string)reader["name"],
            (string)reader["path"],
            (DateOnly)reader["event_date"],
            parentProjectId);
    }
}