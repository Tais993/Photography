using Domain.entities;

namespace Website.Pages.Projects;

public class CreateProjectModalModel
{
    public List<LogicalDrive> ImportDrives { get; set; } = [];
    public LogicalDrive? SelectedImportDrive { get; set; }

    public string? ImportDriveRootPath { get; set; }
    public string? ProjectName { get; set; }
    public DateOnly? ProjectDate { get; set; }

    public bool IsProjectDetailsStep { get; set; }
    
    public string ProjectPath { get; set; } = "";
    

    public string GetImportDriveDisplayName(LogicalDrive drive)
    {
        string displayName = drive.RootPath;

        if (!string.IsNullOrWhiteSpace(drive.VolumeLabel))
        {
            displayName += $" - {drive.VolumeLabel}";
        }

        return displayName;
    }

    public string GetImportDriveBadges(LogicalDrive drive)
    {
        List<string> badges = [];

        if (drive.IsRecommended) badges.Add("Recommended");
        if (drive.HasDcimFolder) badges.Add("DCIM");

        return string.Join(" · ", badges);
    }

    public string GetImportDriveStorage(LogicalDrive drive)
    {
        return $"{FormatBytes(drive.TotalFreeSpace)} free of {FormatBytes(drive.TotalSize)}";
    }

    public string GetDriveInputId(LogicalDrive drive)
    {
        return "create-project-drive-" + drive.RootPath
            .Replace("\\", "")
            .Replace("/", "")
            .Replace(":", "")
            .Replace(" ", "");
    }

    public string GetProjectFolderPreview()
    {
        if (string.IsNullOrWhiteSpace(ProjectName) || ProjectDate is null)
        {
            return "Enter a project name to preview the folder name.";
        }

        return ProjectDate.Value.ToString("yyyy-MM-dd") + "-" + ProjectName;
    }

    // Replace with humanizer dependency?
    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
        {
            return "unknown size";
        }

        string[] sizes = ["B", "KB", "MB", "GB", "TB"];

        double size = bytes;
        int order = 0;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.#} {sizes[order]}";
    }
}