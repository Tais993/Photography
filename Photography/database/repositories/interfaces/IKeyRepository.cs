using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories.interfaces;

public interface IKeyRepository<T, TKey> : IRepository<T, TKey> where T : IEntity
{
    // useless?
}