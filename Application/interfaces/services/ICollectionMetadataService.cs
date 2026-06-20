using Domain.entities;

namespace Application.interfaces.services;

public interface ICollectionMetadataService
{
    Dictionary<string, CollectionMetadataConfiguration> GetCollectionConfigurations();
    CollectionMetadataConfiguration? GetCollectionMetadataConfiguration(string collectionDirectory);
}