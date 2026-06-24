using Domain.entities;

namespace Application.interfaces.services.project;

public interface IProjectScanningService
{
    public void ScanProject(Project project);
}