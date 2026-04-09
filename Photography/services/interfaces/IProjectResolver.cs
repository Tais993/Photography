using PhotographyNET.database.entities;

namespace PhotographyNET.services.interfaces;

public interface IProjectResolver
{
    public Project resolveProject(string directory);

    public void initialiseExistingFolder(string subdirectory);
}