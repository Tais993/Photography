namespace Domain.entities;

public class CollectionMetadataConfiguration
{
    public string[] FolderNames { get; set; } = [];
    public string MetadataKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MetadataValue { get; set; } = "true";
}