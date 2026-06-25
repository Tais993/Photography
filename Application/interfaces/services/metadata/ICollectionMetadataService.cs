using Domain.entities;

namespace Application.interfaces.services.metadata;

public interface ICollectionMetadataService
{
    Dictionary<string, CollectionMetadataConfiguration> GetCollectionConfigurations();
    CollectionMetadataConfiguration? GetCollectionMetadataConfiguration(string collectionDirectory);
}