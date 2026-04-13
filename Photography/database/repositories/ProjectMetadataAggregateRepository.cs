using Npgsql;
using PhotographyNET.database.entities;

namespace PhotographyNET.database.repositories;

public class ProjectMetadataAggregateRepository
{
    private MetadataRepository _metadataRepository;
    private ProjectMetadataRepository _projectMetadataRepository;

    
    private RepositoryHelper _db;
    private ILogger<ProjectMetadataAggregateRepository> _logger;

    
    public ProjectMetadataAggregateRepository(MetadataRepository metadataRepository,
        ProjectMetadataRepository projectMetadataRepository, NpgsqlDataSource dataSource, 
        ILogger<ProjectMetadataAggregateRepository> logger, RepositoryHelper db)
    {
        _metadataRepository = metadataRepository;
        _projectMetadataRepository = projectMetadataRepository;
        this._logger = logger;
        this._db = db;
    }

    public List<ProjectMetadataAggregate> GetAll()
    {
        List<ProjectMetadata> projectMetadatas = _projectMetadataRepository.GetAll();
        Dictionary<int, Metadata> metadatas = _metadataRepository.GetAll().ToDictionary(metadata => metadata.Id ?? throw new Exception("ada"), metadata => metadata);

        List < ProjectMetadataAggregate> projectMetadataAggregates = projectMetadatas.Select(projectMetadata =>
        {
            Metadata metadata = metadatas[projectMetadata.MetadataId];

            return new ProjectMetadataAggregate(projectMetadata, metadata);
        }).ToList();

        return projectMetadataAggregates;
    }

    public ProjectMetadataAggregate Insert(ProjectMetadataAggregate entity)
    {
        _projectMetadataRepository.Insert(entity.ToProjectMetadata());
        return entity;
    }

    public void Update(ProjectMetadataAggregate entity)
    {
        _projectMetadataRepository.Update(entity.ToProjectMetadata());
    }

    public ProjectMetadataAggregate GetByKey(ProjectMetadataIds key)
    {
        ProjectMetadata projectMetadata = _projectMetadataRepository.GetByKey(key) ?? throw new Exception();
        Metadata metadata = _metadataRepository.GetByKey(key.MetadataId) ?? throw new Exception();


        return ToAggregate(metadata, projectMetadata);
    }

    public void DeleteByKey(ProjectMetadataIds key)
    {
        _projectMetadataRepository.DeleteByKey(key);
    }

    private ProjectMetadataAggregate ToAggregate(Metadata m, ProjectMetadata pm)
    {
        return new ProjectMetadataAggregate(pm.ProjectId, pm.MetadataId, pm.MetadataValue, m.MetadataKey, m.MetadataType, m.DisplayName, m.Description);
    }
}