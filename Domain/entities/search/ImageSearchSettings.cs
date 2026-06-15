namespace Domain.entities.search;

public class ImageSearchSettings : SearchSettings
{
    public int? ProjectId = null;
    public string? FileNameOrNumber = null;
    public string? FileName = null;
    public string? FileNumber = null;
    public string? FolderName = null;
    public string? FileType = null;

    public bool HideRawImagesWhenJpgExists = false;
}