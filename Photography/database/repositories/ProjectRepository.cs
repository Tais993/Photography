using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public class ProjectRepository : AbstractRepository<Project>
{
    public ProjectRepository(NpgsqlDataSource dataSource) : base(dataSource)
    {
    }

    public override Project? GetById(int id)
    {
        return QuerySingle("""
                           SELECT id, name, location, event_date FROM public.project 
                           WHERE id = ($1)
                           """, MapProject, id);
    }

    public override List<Project> GetAll()
    {
        return QueryMultiple("""
                             SELECT id, name, location, event_date FROM public.project
                             """, MapProject);
    }

    public override Project Insert(Project project)
    {
        return QuerySingle("""
                           INSERT INTO public.project(name, location, event_date) 
                           VALUES ($1, $2, $3)
                           RETURNING *
                           """, MapProject, project.Name, project.Location, project.EventDate) ??
               throw new Exception("Insert failed");
    }

    public override void Update(Project project)
    {
        if (project?.Id is null) throw new Exception("yeah i need actual stuff");

        Execute("""
                UPDATE public.project
                SET name = $1,
                    location = $2,
                    event_date = $3
                WHERE id = $4
                """, project.Name, project.Location, project.EventDate, project.Id);
    }


    public override Project? DeleteById(int id)
    {
        return QuerySingle("""
                           DELETE FROM public.project 
                           WHERE id = ($1)
                           RETURNING id, name, location, event_date
                           """, MapProject, id);
    }


    private static Project MapProject(NpgsqlDataReader reader)
    {
        return new Project(
            (int)reader["id"],
            (string)reader["name"],
            (string)reader["location"],
            (DateOnly)reader["event_date"]
        );
    }
}