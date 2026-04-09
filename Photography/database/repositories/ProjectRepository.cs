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

    public override Project Insert(Project image)
    {
        return QuerySingle("""
                           INSERT INTO public.project(name, location, date) 
                           VALUES ($1, $2, $3)
                           RETURNING *
                           """, MapProject, image.Name, image.Location, image.Date) ??
               throw new Exception("Insert failed");
    }

    public override void Update(Project image)
    {
        if (image?.Id is null) throw new Exception("yeah i need actual stuff");
        
        Execute("""
                UPDATE public.project
                SET name = $1,
                    location = $2,
                    date = $3
                WHERE id = $4
                """, image.Name, image.Location, image.Date, image.Id);
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
            (string)reader["location"],
            (DateOnly)reader["date"]
        );
    }
}