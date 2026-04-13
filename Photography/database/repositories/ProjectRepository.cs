using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public class ProjectRepository
{
    private RepositoryHelper _db;
    private ILogger<ProjectRepository> _logger;

    public ProjectRepository(NpgsqlDataSource dataSource,
        ILogger<ProjectRepository> logger,
        RepositoryHelper db)
    {
        this._logger = logger;
        this._db = db;
    }

    public Project? GetByKey(int id)
    {
        _logger.LogInformation($"GetByKey, with params: {id}");
        return _db.Query("""
                           SELECT id, name, location, event_date FROM public.project 
                           WHERE id = $1
                           """, MapProject, id);
    }

    public List<Project> GetAll()
    {
        return _db.QueryMultiple("""
                             SELECT id, name, location, event_date FROM public.project
                             """, MapProject);
    }

    public Project Insert(Project project)
    {
        return _db.Query("""
                         INSERT INTO public.project(name, location, event_date) 
                         VALUES ($1, $2, $3)
                         RETURNING *
                         """, MapProject, project.Name, project.Location, project.EventDate) ??
               throw new Exception("Insert failed");
    }

    public void Update(Project project)
    {
        if (project?.Id is null) throw new Exception("yeah i need actual stuff");

        _db.Execute("""
                UPDATE public.project
                SET name = $1,
                    location = $2,
                    event_date = $3
                WHERE id = $4
                """, project.Name, project.Location, project.EventDate, project.Id);
    }


    public void DeleteByKey(int id)
    {
        _db.Execute("""
                           DELETE FROM public.project 
                           WHERE id = ($1)
                           """, id);
    }


    private static Project MapProject(NpgsqlDataReader reader)
    {
        return new Project(
            (int)reader["id"],
            (string)reader["name"],
            (string)reader["location"],
            (DateOnly)reader["event_date"]);
    }
}