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

        try
        {
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
        catch (InvalidOperationException ex)
        {
            return new List<T>();
        }
    }

    public TResult Query<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
        params object?[] parameterValues)
    {
        _logger.LogDebug("Executing {Sql}", sql);

        using NpgsqlConnection cnx = _dataSource.OpenConnection();

        using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        SetParameters<TResult>(parameterValues, npgsqlCommand);

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

        NpgsqlConnection cnx = _dataSource.OpenConnection();


        NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        SetParameters<object>(parameterValues, npgsqlCommand);

        npgsqlCommand.ExecuteNonQuery();

        cnx.CloseAsync();
    }


    private void SetParameters<TResult>(object?[] parameterValues, NpgsqlCommand npgsqlCommand)
    {
        foreach (object? parameterValue in parameterValues)
        {
            _logger.LogDebug("Param: {ParameterValue}", parameterValue);

            if (parameterValue == null)
            {
                npgsqlCommand.Parameters.AddWithValue(DBNull.Value);
            }
            else
            {
                npgsqlCommand.Parameters.AddWithValue(parameterValue);
            }
        }
    }
}