using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public interface IKeyRepository<T, TKey> : IRepository<T, TKey> where T : IEntity
{
    // useless?
}