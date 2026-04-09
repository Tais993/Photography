using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public abstract class AbstractDatabase<T> where T : Entity
{
    public NpgsqlConnection cnx;

    protected AbstractDatabase(NpgsqlConnection cnx)
    {
        this.cnx = cnx;

        cnx.Open();
    }

    public abstract T? GetById(int id);
    public abstract List<T> GetAll();

    public abstract T Insert(T image);
    public abstract void Update(T image);

    public abstract T? DeleteById(int id);

    protected T? QuerySingle(String sql, Func<NpgsqlDataReader, T> resultConverter, params object[] parameterValues)
    {
        var results = QueryMultiple(sql, resultConverter, parameterValues);
        return results.Count > 0 ? results[0] : null;
    }

    protected List<T> QueryMultiple(String sql, Func<NpgsqlDataReader, T> resultConverter, params object[] parameterValues)
    {
        NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        foreach (var parameterValue in parameterValues)
        {
            npgsqlCommand.Parameters.AddWithValue(parameterValue);
        }

        NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();

        List<T> results = [];

        while (reader.Read())
        {
            results.Add(resultConverter(reader));
        }

        reader.Close();

        return results;
    }

    public void Execute(String sql, params object[] parameterValues)
    {
        NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        foreach (var parameterValue in parameterValues)
        {
            npgsqlCommand.Parameters.AddWithValue(parameterValue);
        }

        npgsqlCommand.ExecuteNonQuery();
    }


    void temp()
    {
        NpgsqlCommand npgsqlCommand = new NpgsqlCommand("SELECT * FROM public.project WHERE id = ($1)", cnx);

        npgsqlCommand.Prepare();
        npgsqlCommand.Parameters.AddWithValue(1);
        NpgsqlDataReader npgsqlDataReader = npgsqlCommand.ExecuteReader();


        if (!npgsqlDataReader.Read())
        {
            Console.WriteLine("No rows found.");
            return;
        }

        var id = (int)npgsqlDataReader["id"];
        var name = (String)npgsqlDataReader["name"];
        var location = (String)npgsqlDataReader["location"];

        // var id = npgsqlDataReader.GetInt64(0);
        // var name = npgsqlDataReader.GetString(1);
        // var location = npgsqlDataReader.GetString(2);
        // long? parent_project_id = npgsqlDataReader.IsDBNull(3) ? null : npgsqlDataReader.GetInt64(3);

        Console.WriteLine("id: {0}, name: {1}, location: {2}", id, name, location);
    }
}