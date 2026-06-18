namespace Domain.website;

public class ImageViewModel
{
    public bool Selected { get; set; }
    public int? ImageId { get; set; }
    public string? FileType { get; set; }
    public string? FileName { get; set; }
    public string? RelationalFilePath { get; set; }
}