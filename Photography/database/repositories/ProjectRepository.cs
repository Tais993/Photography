using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public class ProjectDatabase : AbstractDatabase<Project>
{
    public ProjectDatabase(NpgsqlConnection cnx) : base(cnx)
    {
    }

    public override Project? GetById(int id)
    {
        return QuerySingle("""
                          SELECT id, name, location FROM public.project 
                          WHERE id = ($1)
                          """, MapProject, id);
    }
    
    public override List<Project> GetAll()
    {
        return QueryMultiple("""
                           SELECT id, name, location FROM public.project
                           """, MapProject);
    }
    
    public override void Insert(Project project)
    {
        Execute("""
                INSERT INTO public.project(name, location) 
                VALUES ($1, $2)
                """, project.Name, project.Location);
    }

    public override void Update(Project project)
    {
        Execute("""
                UPDATE public.project
                SET name = $1,
                    location = $2
                WHERE id = $3
                """, project.Name, project.Location, project.Id);
    }
    
    
    public override Project? DeleteById(int id)
    {
        return QuerySingle("""
                          DELETE FROM public.project 
                          WHERE id = ($1)
                          RETURNING id, name, location
                          """, MapProject, id);
    }
    
    
    
    private static Project MapProject(NpgsqlDataReader reader)
    {
        return new Project(
            (int)reader["id"],
            (string)reader["name"],
            (string)reader["location"]
        );
    }
}