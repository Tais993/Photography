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

        List<LogicalDrive> logicalDrives = (DriveInfo.GetDrives()
            .Where(driveInfo => driveInfo.IsReady)
            .Where(IsNotCloudDrive)
            .Select(driveInfo => new LogicalDrive
            {
                Name = driveInfo.Name,
                RootPath = driveInfo.RootDirectory.FullName,
                DriveType = driveInfo.DriveType,
                IsReady = driveInfo.IsReady,
                TotalFreeSpace = driveInfo.TotalFreeSpace,
                TotalSize = driveInfo.TotalSize,
                VolumeLabel = driveInfo.VolumeLabel
            })).ToList();

        _logger.LogDebug("Found {Count} logical drives", logicalDrives.Count);
        return logicalDrives;
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
        return driveInfo.Name.Contains(value) ||
               driveInfo.VolumeLabel.Contains(value);
    }

    private static bool ContainsGoogleDriveFolders(DriveInfo driveInfo)
    {
        string root = driveInfo.RootDirectory.FullName;

        return Directory.Exists(Path.Combine(root, "My Drive")) ||
               Directory.Exists(Path.Combine(root, "Mijn Drive")) ||
               Directory.Exists(Path.Combine(root, "Shared drives")) ||
               Directory.Exists(Path.Combine(root, "Gedeelde drives"));
    }
}