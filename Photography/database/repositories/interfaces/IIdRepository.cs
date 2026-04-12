using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories.interfaces;

public interface IIdRepository<T> : IRepository<T, int> where T : IIdEntity
{
    public T? GetById(int id)
    {
        return GetByKey(id);
    }

    public void DeleteById(int id)
    {
        DeleteByKey(id);
    }
}