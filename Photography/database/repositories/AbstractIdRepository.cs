using Npgsql;
using PhotographyNET.database.entities;
using PhotographyNET.database.repositories.interfaces;

namespace PhotographyNET.database.repositories;

public abstract class AbstractIdRepository<T> : AbstractRepository<T, int>, IIdRepository<T> where T : IIdEntity
{
    protected AbstractIdRepository(NpgsqlDataSource dataSource, ILogger<IRepository<T, int>> logger) : base(dataSource, logger)
    {
    }

    public T? GetById(int id)
    {
        return GetByKey(id);
    }

    public void DeleteById(int id)
    {
        DeleteByKey(id);
    }
}