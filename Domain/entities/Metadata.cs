namespace Domain.entities;

public class Metadata(int? id, string metadataKey, string metadataType, string displayName, string description)
    : IEntity
{
    public string Description = description;
    public string DisplayName = displayName;
    public string MetadataKey = metadataKey;
    public string MetadataType = metadataType;
    public int? Id { get; } = id;
}