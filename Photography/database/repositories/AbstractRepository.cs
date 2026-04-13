using Npgsql;
using PhotographyNET.database.entities;
using PhotographyNET.database.repositories.interfaces;

namespace PhotographyNET.database.repositories;

public abstract class AbstractRepository<T, TKey> : IRepository<T, TKey> where T : IEntity
{
    private readonly NpgsqlDataSource _dataSource;
    private ILogger<IRepository<T, TKey>> _logger;

    protected AbstractRepository(NpgsqlDataSource dataSource, ILogger<IRepository<T, TKey>> logger)
    {
        this._dataSource = dataSource;
        this._logger = logger;
    }

    protected T? QuerySingle(string sql, Func<NpgsqlDataReader, T> resultConverter, params object[] parameterValues)
    {
        _logger.LogInformation($"QueryMultiple, with params: {parameterValues}");
        return Query(sql, resultConverter, parameterValues);
    }

    protected List<T> QueryMultiple(string sql, Func<NpgsqlDataReader, T> resultConverter,
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

    protected TResult Query<TResult>(string sql, Func<NpgsqlDataReader, TResult> resultConverter,
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

    public abstract List<T> GetAll();
    public abstract T Insert(T entity);
    public abstract void Update(T entity);
    public abstract T? GetByKey(TKey key);
    public abstract void DeleteByKey(TKey key);
}