namespace Domain.entities;

public class Metadata(int id, string metadataKey, string metadataType, string displayName, string description)
    : IIdEntity
{
    public int? Id { get; } = id;
    public string MetadataKey = metadataKey;
    public string MetadataType = metadataType;
    public string DisplayName = displayName;
    public string Description = description;
}