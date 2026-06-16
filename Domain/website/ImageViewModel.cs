namespace Domain.website;

public class ImageViewModel
{
    public bool Selected { get; init; }
    public int? ImageId { get; init; }
    public string? FileType { get; init; }
    public string? FileName { get; init; }
    public string? RelationalFilePath { get; init; }
}