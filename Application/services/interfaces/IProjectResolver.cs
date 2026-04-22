using Domain.entities;

namespace Application.services.interfaces;

public interface IProjectResolver
{
    public Project resolveProject(string directory);

    public void initialiseExistingFolder(string subdirectory);
}