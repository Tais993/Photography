using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public interface IKeyRepository<T, TKey> : IRepository<T> where T : IEntity
{
    T? GetByKey(TKey key);
    T? DeleteByKey(TKey key);
}