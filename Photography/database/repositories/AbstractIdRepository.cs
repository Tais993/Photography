using Npgsql;
using PhotographyNET.database.entities;
using PhotographyNET.database.repositories.interfaces;

namespace PhotographyNET.database.repositories;

public abstract class AbstractIdRepository<T> : AbstractRepository<T>, IIdRepository<T> where T : IIdEntity
{
    protected AbstractIdRepository(NpgsqlDataSource dataSource) : base(dataSource)
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

    public abstract List<T> GetAll();
    public abstract T Insert(T entity);
    public abstract void Update(T entity);
    public abstract T? GetByKey(int key);
    public abstract void DeleteByKey(int key);
}