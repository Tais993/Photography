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
}