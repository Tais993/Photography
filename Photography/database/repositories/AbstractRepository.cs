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

    protected T? QuerySingle(string sql, Func<NpgsqlDataReader, T> resultConverter, params object[] parameterValues)
    {
        return Query(sql, resultConverter);
    }

    protected List<T> QueryMultiple(string sql, Func<NpgsqlDataReader, T> resultConverter,
        params object[] parameterValues)
    {
        return Query(sql, reader =>
        {
            List<T> results = [];

            while (reader.Read())
            {
                results.Add(resultConverter(reader));
            }

            return results;
        });
    }

    protected TResult Query<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
        params object[] parameterValues)
    {
        NpgsqlConnection cnx = this.dataSource.OpenConnection();

        NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        foreach (var parameterValue in parameterValues)
        {
            npgsqlCommand.Parameters.AddWithValue(parameterValue);
        }

        NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();

        var tResult = resultConverter(reader);

        reader.CloseAsync();
        cnx.CloseAsync();

        return tResult;
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