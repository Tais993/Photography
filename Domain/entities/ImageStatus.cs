namespace Domain.entities;

public enum ImageStatus
{
    Available,
    Unavailable,
    Hidden,
    Deleted
}

public static class ImageStatusMapper
{
    public static string ToDatabaseValue(ImageStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }

    public static ImageStatus ToImageStatus(string? value)
    {
        if (Enum.TryParse(value, true, out ImageStatus status))
        {
            return status;
        }

        return ImageStatus.Available;
    }
}