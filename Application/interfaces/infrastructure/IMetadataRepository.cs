using Domain.entities;

namespace Application.interfaces;

public interface IMetadataRepository
{
    Metadata? GetByKey(string key);
    List<Metadata> GetAll();
    Metadata Insert(Metadata entity);
    void Update(Metadata entity);
    void DeleteById(string key);
}