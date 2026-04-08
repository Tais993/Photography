namespace PhotographyNET.database.entities;

public class Metadata(int id, string metadataKey, string metadataType, string displayName, string description) : Entity(id)
{
    public string MetadataKey { get; } = metadataKey;
    public string MetadataType { get; } = metadataType;
    public string DisplayName { get; } = displayName;
    public string Description { get; } = description;
}