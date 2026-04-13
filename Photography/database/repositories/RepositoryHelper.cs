using Npgsql;

namespace PhotographyNET.database.repositories;

public class RepositoryHelper
{
    private readonly NpgsqlDataSource _dataSource;
    private ILogger<RepositoryHelper> _logger;

    public RepositoryHelper(NpgsqlDataSource dataSource, ILogger<RepositoryHelper> logger)
    {
        this._dataSource = dataSource;
        this._logger = logger;
    }
    
    
    public List<T> QueryMultiple<T>(string sql, Func<NpgsqlDataReader, T> resultConverter,
        params object[] parameterValues)
    {
        _logger.LogInformation($"QueryMultiple, with params: {parameterValues}");
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
        _logger.LogInformation($"Executing {sql}");

        foreach (var parameterValue in parameterValues)
        {
            _logger.LogInformation($"Param: {parameterValue}");
        }


        NpgsqlConnection cnx = this._dataSource.OpenConnection();

        NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        foreach (var parameterValue in parameterValues)
        {
            npgsqlCommand.Parameters.AddWithValue(parameterValue);
        }

        NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();

        reader.Read();

        var tResult = resultConverter(reader);

        reader.CloseAsync();
        cnx.CloseAsync();

        return tResult;
    }

    public void Execute(string sql, params object[] parameterValues)
    {
        NpgsqlConnection cnx = this._dataSource.OpenConnection();


        NpgsqlCommand npgsqlCommand = new NpgsqlCommand(sql, cnx);

        foreach (var parameterValue in parameterValues)
        {
            npgsqlCommand.Parameters.AddWithValue(parameterValue);
        }

        npgsqlCommand.ExecuteNonQuery();

        cnx.CloseAsync();
    }
}