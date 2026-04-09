using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public interface IIdRepository<T> : IRepository<T> where T : IIdEntity
{
    public T? GetById(int id);
    public T? DeleteById(int id);
}