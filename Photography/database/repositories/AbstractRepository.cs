using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public abstract class AbstractRepository<T> where T : Entity
{
    public NpgsqlDataSource dataSource;

    protected AbstractRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public abstract T? GetById(int id);
    public abstract List<T> GetAll();

    public abstract T Insert(T image);
    public abstract void Update(T image);

    public abstract T? DeleteById(int id);

    protected T? QuerySingle(string sql, Func<NpgsqlDataReader, T> resultConverter, params object[] parameterValues)
    {
        var results = QueryMultiple(sql, resultConverter, parameterValues);
        return results.Count > 0 ? results[0] : null;
    }

    protected List<T> QueryMultiple(string sql, Func<NpgsqlDataReader, T> resultConverter, params object[] parameterValues)
    {
        NpgsqlConnection cnx = this.dataSource.OpenConnection();

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

        reader.CloseAsync();
        cnx.CloseAsync();

        return results;
    }

    public void Execute(string sql, params object[] parameterValues)
    {
        NpgsqlConnection cnx = this.dataSource.OpenConnection();


        NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        foreach (var parameterValue in parameterValues)
        {
            npgsqlCommand.Parameters.AddWithValue(parameterValue);
        }

        npgsqlCommand.ExecuteNonQuery();

        cnx.CloseAsync();
    }
}