using Domain.entities;

namespace Infrastructure.database.repositories;

public interface IMetadataRepository
{
    Metadata? GetById(int id);
    Metadata? GetByKey(string key);
    List<Metadata> GetAll();
    Metadata Insert(Metadata entity);
    void Update(Metadata entity);
    void DeleteById(int id);
}