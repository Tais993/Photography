namespace Domain.entities;

public class LogicalDrive
{
    public string Name { get; set; } = string.Empty;
    public string VolumeLabel { get; set; } = string.Empty;
    public string RootPath { get; set; } = string.Empty;
    public DriveType DriveType { get; set; }
    public bool IsReady { get; set; }
    
    public string ImageFolderPath { get; set; } = string.Empty;
    
    public long TotalSize { get; set; }
    public long TotalFreeSpace { get; set; }
    
    public bool HasDcimFolder { get; set; }
    public bool HasCameraBrand { get; set; }
    public bool HasCameraFolder { get; set; }
    public bool IsRecommended { get; set; }
}