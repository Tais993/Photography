using Application.interfaces.infrastructure;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.filesystem;

public class LogicalDriveRepository : ILogicalDriveRepository
{
    private readonly ILogger<LogicalDriveRepository> _logger;

    public LogicalDriveRepository(ILogger<LogicalDriveRepository> logger)
    {
        _logger = logger;
    }

    public List<LogicalDrive> GetAllReady()
    {
        _logger.LogDebug("Getting logical drives");

        foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
        {
            Console.WriteLine("RootDirectory: " + driveInfo.RootDirectory.FullName);
        }

        List<LogicalDrive> logicalDrives = (DriveInfo.GetDrives()
            .Where(IsUsefulDrive)
            .Where(IsNotCloudDrive)
            .Select(MapLogicalDrive)
            .ToList());

        _logger.LogDebug("Found {Count} logical drives", logicalDrives.Count);

        return logicalDrives;
    }

    private LogicalDrive MapLogicalDrive(DriveInfo driveInfo)
    {
        string rootPath = NormalizeLinuxMountPath(driveInfo.RootDirectory.FullName);
        string name = NormalizeLinuxMountPath(driveInfo.Name);

        return new LogicalDrive
        {
            Name = name,
            RootPath = rootPath,
            DriveType = driveInfo.DriveType,
            IsReady = driveInfo.IsReady,
            TotalFreeSpace = TryGetLong(() => driveInfo.TotalFreeSpace),
            TotalSize = TryGetLong(() => driveInfo.TotalSize),
            VolumeLabel = TryGetString(() => driveInfo.VolumeLabel)
        };
    }

    /// <summary>
    /// Verifies drive readiness. On Linux, only likely external mount paths are accepted.
    /// On other operating systems, removable and fixed drives are accepted.
    /// </summary>
    private bool IsUsefulDrive(DriveInfo driveInfo)
    {
        if (OperatingSystem.IsLinux())
        {
            string rootPath = NormalizeLinuxMountPath(driveInfo.RootDirectory.FullName);

            bool isExternalMount =
                rootPath.StartsWith("/media/", StringComparison.OrdinalIgnoreCase) ||
                rootPath.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase) ||
                rootPath.StartsWith("/run/media/", StringComparison.OrdinalIgnoreCase);

            return isExternalMount && Directory.Exists(rootPath);
        }

        if (!driveInfo.IsReady)
        {
            return false;
        }

        return driveInfo.DriveType is DriveType.Removable or DriveType.Fixed;
    }


    private static readonly string GoogleDrive = "Google Drive";
    private static readonly string Google = "Google";

    private bool IsNotCloudDrive(DriveInfo driveInfo)
    {
        return !(ContainsString(driveInfo, GoogleDrive) ||
                 ContainsString(driveInfo, Google) ||
                 ContainsGoogleDriveFolders(driveInfo));
    }

    private static bool ContainsString(DriveInfo driveInfo, string value)
    {
        string name = NormalizeLinuxMountPath(driveInfo.Name);
        string volumeLabel = TryGetString(() => driveInfo.VolumeLabel);

        return name.Contains(value, StringComparison.OrdinalIgnoreCase) ||
               volumeLabel.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsGoogleDriveFolders(DriveInfo driveInfo)
    {
        string root = NormalizeLinuxMountPath(driveInfo.RootDirectory.FullName);

        return Directory.Exists(Path.Combine(root, "My Drive")) ||
               Directory.Exists(Path.Combine(root, "Mijn Drive")) ||
               Directory.Exists(Path.Combine(root, "Shared drives")) ||
               Directory.Exists(Path.Combine(root, "Gedeelde drives"));
    }

    private static long TryGetLong(Func<long> getValue)
    {
        try
        {
            return getValue();
        }
        catch
        {
            return 0;
        }
    }

    private static string TryGetString(Func<string> getValue)
    {
        try
        {
            return getValue();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string NormalizeLinuxMountPath(string path)
    {
        if (!OperatingSystem.IsLinux())
        {
            return path;
        }

        return path.Replace("\\040", " ");
    }
}