namespace Domain.entities;

public class ImageMetadata
{
    public ImageMetadata(int imageId, string metadataKey, string metadataValue, string metadataType, string displayName,string description)
    {
        ImageId = imageId;
        MetadataKey = metadataKey;
        MetadataValue = metadataValue;
        MetadataType = metadataType;
        DisplayName = displayName;
        Description = description;
    }
    
    public ImageMetadata(int imageId, Metadata metadata, string? metadataValue)
    {
        ImageId = imageId;
        MetadataKey = metadata.MetadataKey;
        MetadataValue = metadataValue;
        MetadataType = metadata.MetadataType;
        DisplayName = metadata.DisplayName;
        Description = metadata.Description;
    }
    
    public ImageMetadata(int imageId, string metadataKey, string? metadataValue)
    {
        ImageId = imageId;

        MetadataKey = metadataKey;
        MetadataValue = metadataValue;

        MetadataType = string.Empty;
        DisplayName = string.Empty;
        Description = string.Empty;
    }


    public int ImageId { get; set; }
    public string MetadataKey { get; set; }
    public string? MetadataValue { get; set; }
    public string MetadataType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
}