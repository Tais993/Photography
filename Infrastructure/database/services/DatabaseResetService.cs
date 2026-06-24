using Application.interfaces.infrastructure.services;
using Infrastructure.database.repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.database.services;

public class DatabaseResetService : IDatabaseResetService
{
    private readonly RepositoryHelper _repositoryHelper;
    private readonly ILogger<DatabaseResetService> _logger;

    public DatabaseResetService(RepositoryHelper repositoryHelper, ILogger<DatabaseResetService> logger)
    {
        _repositoryHelper = repositoryHelper;
        _logger = logger;
    }

    public void DropDatabase()
    {
        _logger.LogWarning("All application database tables are going to be dropped");
        
        _repositoryHelper.Execute("""
                                  DROP TABLE IF EXISTS selection_session_image CASCADE;
                                  DROP TABLE IF EXISTS selection_session CASCADE;
                                  DROP TABLE IF EXISTS project_metadata CASCADE;
                                  DROP TABLE IF EXISTS image_metadata CASCADE;
                                  DROP TABLE IF EXISTS metadata CASCADE;
                                  DROP TABLE IF EXISTS image CASCADE;
                                  DROP TABLE IF EXISTS project CASCADE;
                                  DROP TABLE IF EXISTS changelog CASCADE
                                  """);
        
        _logger.LogWarning("All application database tables have been dropped");
    }
}