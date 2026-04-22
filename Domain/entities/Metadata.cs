namespace Domain.entities;

public class Metadata(int id, string metadataKey, string metadataType, string displayName, string description) : IIdEntity
{
    public int? Id { get; } = id;
    public string MetadataKey { get; } = metadataKey;
    public string MetadataType { get; } = metadataType;
    public string DisplayName { get; } = displayName;
    public string Description { get; } = description;
}