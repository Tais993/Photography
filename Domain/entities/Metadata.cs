namespace Domain.entities;

public class Metadata(string metadataKey, string metadataType, string displayName, string description)
{
    public string Description = description;
    public string DisplayName = displayName;
    public string MetadataKey = metadataKey;
    public string MetadataType = metadataType;
}