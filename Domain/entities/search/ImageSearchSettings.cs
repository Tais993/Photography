namespace Domain.entities.search;

public class ImageSearchSettings : SearchSettings
{
    public int? ProjectId { get; set; }
    public string? FileNameOrNumber { get; set; }
    public string? FileName { get; set; }
    public string? FileNumber { get; set; }
    public string? FolderName { get; set; }
    public string? FileType { get; set; }

    public bool HideRawFilesWhenImageExists { get; set; }
}