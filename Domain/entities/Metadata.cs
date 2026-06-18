namespace Domain.entities;

public class Metadata
{
    public Metadata(string metadataKey, string metadataType, string displayName, string description)
    {
        MetadataKey = metadataKey;
        MetadataType = metadataType;
        DisplayName = displayName;
        Description = description;
    }

    public string MetadataKey { get; }
    public string MetadataType { get; set;}
    public string DisplayName { get; set;}
    public string Description { get; set;}
}