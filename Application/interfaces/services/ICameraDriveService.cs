using Domain.entities;

namespace Application.interfaces.services;

public interface ICameraDriveService
{
    public List<LogicalDrive> GetImportDrives();
    
    public DateTime? GetFirstPhotoDate(LogicalDrive logicalDrive);
    public List<String> GetFilesToCopy(LogicalDrive logicalDrive);
    
}