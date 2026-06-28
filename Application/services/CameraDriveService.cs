using System.Globalization;
using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using ImageMagick;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services;

public class CameraDriveService : ICameraDriveService
{
    private readonly ILogicalDriveRepository _logicalDriveRepository;
    private readonly ILogger<CameraDriveService> _logger;
    private readonly IFiles _files;

    public CameraDriveService(ILogicalDriveRepository logicalDriveRepository, IFiles files, ILogger<CameraDriveService> logger)
    {
        _logicalDriveRepository = logicalDriveRepository;
        _files = files;
        _logger = logger;
    }

    public List<LogicalDrive> GetImportDrives()
    {
        _logger.LogDebug("Getting import drives");

        List<LogicalDrive> logicalDrives = _logicalDriveRepository
            .GetAllReady()
            .Select(SetCameraDriveProperties)
            .ToList();

        List<LogicalDrive> orderedDrives = logicalDrives
            .OrderByDescending(logicalDrive => logicalDrive.HasDcimFolder)
            .ThenByDescending(logicalDrive => logicalDrive.IsRecommended)
            .ThenByDescending(logicalDrive => logicalDrive.DriveType == DriveType.Removable)
            .ThenByDescending(logicalDrive => logicalDrive.HasCameraBrand)
            .ThenByDescending(logicalDrive => logicalDrive.HasCameraFolder)
            .ThenBy(logicalDrive => logicalDrive.Name)
            .ToList();

        _logger.LogDebug("Found {Count} import drive options", orderedDrives.Count);
        return orderedDrives;
    }

    /// <summary>
    /// Gets the first photo date from the drive.
    /// </summary>
    /// <param name="logicalDrive"></param>
    /// <returns></returns>
    public DateTime? GetFirstPhotoDate(LogicalDrive logicalDrive)
    {
        string? firstImageFile = _files
            .EnumerateFiles(logicalDrive.ImageFolderPath, "*", SearchOption.AllDirectories)
            .Take(100)
            .FirstOrDefault(file => ImageFileTypes.Contains(_files.GetFileExtension(file)));

        if (firstImageFile is null)
        {
            return null;
        }

        return GetDateTaken(firstImageFile);
    }
    
    private DateTime? GetDateTaken(string imagePath)
    {
        using MagickImage image = new(imagePath);

        IExifProfile? exifProfile = image.GetExifProfile();

        if (exifProfile is null)
        {
            return null;
        }

        IExifValue<string>? dateTaken = exifProfile.GetValue(ExifTag.DateTimeOriginal);

        if (dateTaken?.Value is null)
        {
            return null;
        }

        if (!DateTime.TryParseExact(
                dateTaken.Value,
                "yyyy:MM:dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate))
        {
            return null;
        }

        return parsedDate;
    }
    
    public List<string> GetFilesToCopy(LogicalDrive logicalDrive)
    {
        return _files.GetFiles(logicalDrive.ImageFolderPath, "*", SearchOption.AllDirectories)
            .Where(file => SupportedImageOrRawFileTypes.Contains(_files.GetFileExtension(file)))
            .ToList();
    }

    private string GetDefaultPhotoFolder(LogicalDrive logicalDrive)
    {
        string dcimFolder = GetDcimFolder(logicalDrive.RootPath);

        if (_files.Exists(dcimFolder))
        {
            return dcimFolder;
        }

        return logicalDrive.RootPath;
    }

    private LogicalDrive SetCameraDriveProperties(LogicalDrive logicalDrive)
    {
        logicalDrive.HasDcimFolder = HasDcimFolder(logicalDrive.RootPath);
        logicalDrive.HasCameraBrand = HasCameraBrand(logicalDrive);
        logicalDrive.HasCameraFolder = HasCameraFolder(logicalDrive.RootPath);
        logicalDrive.IsRecommended = IsRecommended(logicalDrive);

        logicalDrive.ImageFolderPath = GetDefaultPhotoFolder(logicalDrive);

        return logicalDrive;
    }

    private bool IsRecommended(LogicalDrive logicalDrive)
    {
        if (logicalDrive.HasDcimFolder)
        {
            return true;
        }

        switch (logicalDrive.DriveType == DriveType.Removable)
        {
            case true when logicalDrive.HasCameraBrand:
            case true when logicalDrive.HasCameraFolder:
                return true;
        }

        return logicalDrive.HasCameraBrand & logicalDrive.HasCameraFolder;
    }


    private bool HasDcimFolder(string rootPath)
    {
        return _files.Exists(GetDcimFolder(rootPath));
    }

    private bool HasCameraBrand(LogicalDrive logicalDrive)
    {
        return CameraBrands.Any(cameraBrand =>
            logicalDrive.Name.Contains(cameraBrand, StringComparison.OrdinalIgnoreCase) ||
            logicalDrive.VolumeLabel.Contains(cameraBrand, StringComparison.OrdinalIgnoreCase));
    }


    private bool HasCameraFolder(string rootPath)
    {
        foreach (string folderName in CameraDriveFolderNames)
        {
            if (_files.Exists(_files.Combine(rootPath, folderName)))
            {
                return true;
            }
        }

        return false;
    }

    private string GetDcimFolder(string rootPath)
    {
        return _files.Combine(rootPath, CameraDriveDcimFolderName);
    }
}