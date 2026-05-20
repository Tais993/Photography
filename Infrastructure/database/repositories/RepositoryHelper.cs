using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.database.repositories;

public class RepositoryHelper
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<RepositoryHelper> _logger;

    public RepositoryHelper(NpgsqlDataSource dataSource, ILogger<RepositoryHelper> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }


    public List<T> QueryMultiple<T>(string sql, Func<NpgsqlDataReader, T> resultConverter,
        params object[] parameterValues)
    {
        _logger.LogDebug("QueryMultiple, with params: {ParameterValues}", (object?)parameterValues);
        return Query(sql, reader =>
        {
            List<T> results = [];

            do
            {
                results.Add(resultConverter(reader));
            } while (reader.Read());

            return results;
        }, parameterValues);
    }

    public TResult Query<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
        params object[] parameterValues)
    {
        _logger.LogDebug("Executing {Sql}", sql);

        foreach (object parameterValue in parameterValues)
        {
            _logger.LogDebug("Param: {ParameterValue}", parameterValue);
        }


        using NpgsqlConnection cnx = _dataSource.OpenConnection();

        using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        foreach (object parameterValue in parameterValues)
        {
            npgsqlCommand.Parameters.AddWithValue(parameterValue);
        }

        using NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();

        if (!reader.Read())
        {
            throw new InvalidOperationException("Query returned no results");
        }

        TResult tResult = resultConverter(reader);

        return tResult;
    }

    public void Execute(string sql, params object[] parameterValues)
    {
        _logger.LogDebug("Executing {Sql}", sql);

        foreach (object parameterValue in parameterValues)
        {
            _logger.LogDebug("Param: {ParameterValue}", parameterValue);
        }

        NpgsqlConnection cnx = _dataSource.OpenConnection();


        NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        foreach (object parameterValue in parameterValues)
        {
            npgsqlCommand.Parameters.AddWithValue(parameterValue);
        }

        npgsqlCommand.ExecuteNonQuery();

        cnx.CloseAsync();
    }
}