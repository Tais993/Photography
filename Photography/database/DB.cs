using EvolveDb;
using Npgsql;
using PhotographyNET.database.repositories;

namespace PhotographyNET.database;

public class DB
{
    private ILogger logger = Log.Factory.CreateLogger("database");

    public void test(string connectionString)
    {
        try
        {
            var cnx = new NpgsqlConnection(connectionString);
            var evolve = new Evolve(cnx)
            {
                Locations = new[] { "database/migrations" },
            };
            
            evolve.Migrate();
            
            
            
            
            ProjectDatabase projectDatabase = new  ProjectDatabase(cnx);
            
            // Project? project1 = database.WithReturnV1("SELECT * FROM public.project WHERE id = ($1)", collection =>
            // {
            //     collection.AddWithValue(1);
            // }, reader =>
            // {
            //     var id = (int) reader["id"];
            //     var name = (String) reader["name"];
            //     var location = (String) reader["location"];
            //
            //     return new Project(id, name, location);
            // });
            
            // Project? project2 = projectDatabase.WithReturn("SELECT * FROM public.project WHERE id = ($1)", reader =>
            // {
            //     var id = (int) reader["id"];
            //     var name = (String) reader["name"];
            //     var location = (String) reader["location"];
            //
            //     return new Project(id, name, location);
            // }, 1);
            
            // Console.WriteLine(project1);

            // Console.WriteLine("all projects: ");
            //
            // foreach (var project in projectDatabase.GetAll())
            // {
            //     Console.WriteLine(project);
            // }

             // var project = projectDatabase.GetById(1);
             // Console.WriteLine("from DB: " + project);


            // cnx.Open();
            // NpgsqlCommand npgsqlCommand = new NpgsqlCommand("SELECT * FROM public.project WHERE id = 1", cnx);
            //
            // npgsqlCommand.Prepare();
            // // npgsqlCommand.Parameters.AddWithValue("@id", 1);
            // NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();
            //
            //
            // if (!npgsqlDataReader.Read())
            // {
            //     Console.WriteLine("No rows found.");
            //     return;
            // }
            //
            // var id = (int) npgsqlDataReader["id"];
            // var name = (String) npgsqlDataReader["name"];
            // var location = (String) npgsqlDataReader["location"];
            //
            // // var id = npgsqlDataReader.GetInt64(0);
            // // var name = npgsqlDataReader.GetString(1);
            // // var location = npgsqlDataReader.GetString(2);
            // // long? parent_project_id = npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetInt64(3);
            //
            // Console.WriteLine("id: {0}, name: {1}, location: {2}", id, name, location);
        }
        catch (Exception ex)
        {
            // logger.LogCritical("Database migration failed.", ex);
            throw;
        }

    }
}