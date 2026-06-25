using Domain.entities;

namespace Application.interfaces.services.metadata;

public interface IMetadataService
{
    public Metadata CreateMetadata(string metadataKey, string metadataType, string displayName, string description);
    public Metadata CreateMetadata(Metadata metadata);

    public void UpdateMetadata(string metadataKey, string metadataType, string displayName, string description);
    public void UpdateMetadata(Metadata metadata);

    public Metadata? GetMetadata(Metadata metadata);
    public Metadata? GetMetadata(string metadataKey);

    public void DeleteMetadata(Metadata metadata);
    public void DeleteMetadata(string metadataKey);
}