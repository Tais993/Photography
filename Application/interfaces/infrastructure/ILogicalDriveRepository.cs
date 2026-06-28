using Domain.entities;

namespace Application.interfaces.infrastructure;

public interface ILogicalDriveRepository
{
    
    /// <summary>
    /// Returns a list of all connected drives with their name, type and availability.
    /// </summary>
    /// <returns></returns>
    public List<LogicalDrive> GetAllReady();
}