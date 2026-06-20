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

        List<T>? results = QueryInternal(sql, reader =>
        {
            List<T> results = [];

            do
            {
                results.Add(resultConverter(reader));
            } while (reader.Read());

            _logger.LogDebug("Query returned {Count} results", results.Count);

            return results;
        }, false, parameterValues);

        return results ?? [];
    }

    public TResult? QueryOrDefault<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
        params object?[] parameterValues)
    {
        return QueryInternal(sql, resultConverter, false, parameterValues);
    }

    public TResult Query<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
        params object?[] parameterValues)
    {
        TResult? result = QueryInternal(sql, resultConverter, true, parameterValues);

        return result!;
    }

    private TResult? QueryInternal<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
        bool throwWhenNoResult, params object?[] parameterValues)
    {
        _logger.LogTrace("Executing query with {ParameterCount} parameters: {Sql}", parameterValues.Length, sql);

        using NpgsqlConnection cnx = _dataSource.OpenConnection();

        using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        SetParameters(parameterValues, npgsqlCommand);

        using NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();

        if (!reader.Read())
        {
            _logger.LogTrace("Query returned no results: {Sql}", sql);

            if (throwWhenNoResult)
            {
                throw new InvalidOperationException("Query returned no results");
            }

            return default;
        }

        return resultConverter(reader);
    }
    

    public TResult? QueryScalarOrDefault<TResult>(string sql, params object?[] parameterValues)
    {
        return QueryScalarInternal<TResult>(sql, false, parameterValues);
    }

    public TResult QueryScalar<TResult>(string sql, params object?[] parameterValues)
    {
        TResult? result = QueryScalarInternal<TResult>(sql, true, parameterValues);

        return result!;
    }

    private TResult? QueryScalarInternal<TResult>(string sql, bool throwWhenNoResult,
        params object?[] parameterValues)
    {
        _logger.LogTrace("Executing scalar query with {ParameterCount} parameters: {Sql}", parameterValues.Length, sql);

        using NpgsqlConnection cnx = _dataSource.OpenConnection();

        using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        SetParameters(parameterValues, npgsqlCommand);

        object? result = npgsqlCommand.ExecuteScalar();

        if (result is null || result == DBNull.Value)
        {
            _logger.LogTrace("Scalar query returned no result: {Sql}", sql);

            if (throwWhenNoResult)
            {
                throw new InvalidOperationException("Scalar query returned no results");
            }

            return default;
        }

        return (TResult)Convert.ChangeType(result, typeof(TResult));
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