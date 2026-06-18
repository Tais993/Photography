namespace Domain.entities;

public class ThumbnailResult
{
    private ThumbnailResult(bool found, string? filePath, string contentType)
    {
        Found = found;
        FilePath = filePath;
        ContentType = contentType;
    }

    public bool Found { get; }
    public string? FilePath { get; }
    public string ContentType { get; }

    public static ThumbnailResult NotFound()
    {
        return new ThumbnailResult(false, null, "image/jpeg");
    }

    public static ThumbnailResult Success(string filePath)
    {
        return new ThumbnailResult(true, filePath, "image/jpeg");
    }
}