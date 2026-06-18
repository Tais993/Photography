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
        params object?[] parameterValues)
    {
        _logger.LogDebug("Executing query expecting multiple results with {ParameterCount} parameters", parameterValues.Length);

        try
        {
            return Query(sql, reader =>
            {
                List<T> results = [];

                do
                {
                    results.Add(resultConverter(reader));
                } while (reader.Read());

                _logger.LogDebug("Query returned {Count} results", results.Count);

                return results;
            }, parameterValues);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "Query returned no results");
            return [];
        }
    }

    public TResult? QueryOrDefault<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
        params object?[] parameterValues)
    {
        _logger.LogTrace("Executing query with {ParameterCount} parameters: {Sql}", parameterValues.Length, sql);

        using NpgsqlConnection cnx = _dataSource.OpenConnection();

        using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        SetParameters(parameterValues, npgsqlCommand);

        using NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();

        if (!reader.Read())
        {
            _logger.LogTrace("Query returned no results: {Sql}", sql);
            return default;
        }

        return resultConverter(reader);
    }
    
    public TResult Query<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
        params object?[] parameterValues)
    {
        _logger.LogTrace("Executing query with {ParameterCount} parameters: {Sql}", parameterValues.Length, sql);

        using NpgsqlConnection cnx = _dataSource.OpenConnection();

        using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        SetParameters(parameterValues, npgsqlCommand);

        using NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();

        if (!reader.Read())
        {
            _logger.LogTrace("Query returned no results: {Sql}", sql);
            throw new InvalidOperationException("Query returned no results");
        }

        return resultConverter(reader);
    }

    public void Execute(string sql, params object[] parameterValues)
    {
        _logger.LogTrace("Executing command with {ParameterCount} parameters: {Sql}", parameterValues.Length, sql);

        using NpgsqlConnection cnx = _dataSource.OpenConnection();

        using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        SetParameters(parameterValues, npgsqlCommand);

        int affectedRows = npgsqlCommand.ExecuteNonQuery();

        _logger.LogDebug("Command executed. Affected rows: {AffectedRows}", affectedRows);
    }


    private void SetParameters(object?[] parameterValues, NpgsqlCommand npgsqlCommand)
    {
        foreach (object? parameterValue in parameterValues)
        {
            _logger.LogTrace("Adding query parameter: {ParameterValue}", parameterValue);

            npgsqlCommand.Parameters.AddWithValue(parameterValue ?? DBNull.Value);
        }
    }

    public int MapToInt(NpgsqlDataReader reader)
    {
        return !reader.HasRows ? 0 : reader.GetInt32(0);
    }


    public bool MapToBool(NpgsqlDataReader reader)
    {
        return reader.HasRows && reader.GetBoolean(0);
    }
}