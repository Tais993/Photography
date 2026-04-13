using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories.interfaces;

public interface IRepository<T, TKey> where T : IEntity
{
    public List<T> GetAll();

    public T Insert(T entity);
    public void Update(T entity);

    public T? GetByKey(TKey key);
    public void DeleteByKey(TKey key);
}