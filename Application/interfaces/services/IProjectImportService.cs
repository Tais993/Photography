using Domain.entities;

namespace Application.interfaces.services;

public interface IProjectImportService
{
    Guid StartImport(ProjectImportRequest request);
    ProjectImportProgress? GetProgress(Guid importId);
}