namespace Domain.website;

public class ProjectViewModel
{
    public bool Selected;
    public int? Id;
    public string? Name;
    public string? Path;
    public DateOnly? EventDate;
    
    public long? StorageTotalBytes;
    public long? StorageLocalBytes;

    public bool HasStorageInfo => StorageTotalBytes is not null;

    public string StorageSummary
    {
        get
        {
            if (StorageTotalBytes is null)
            {
                return "Storage not calculated";
            }

            return $"{FormatBytes(StorageLocalBytes ?? 0)} local / {FormatBytes(StorageTotalBytes.Value)} total";
        }
    }
    
    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];

        double size = bytes;
        int unit = 0;

        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        if (unit == 0)
        {
            return $"{bytes} B";
        }

        return $"{size:0.#} {units[unit]}";
    }
}
