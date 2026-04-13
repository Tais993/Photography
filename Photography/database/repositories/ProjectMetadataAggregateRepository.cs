using Npgsql;
using PhotographyNET.database.entities;
using PhotographyNET.database.repositories.interfaces;

namespace PhotographyNET.database.repositories;

public class ProjectMetadataAggregateRepository :
    AbstractRepository<ProjectMetadataAggregate, ProjectMetadataIds>,
    IKeyRepository<ProjectMetadataAggregate, ProjectMetadataIds>
{
    private MetadataRepository _metadataRepository;
    private ProjectMetadataRepository _projectMetadataRepository;

    public ProjectMetadataAggregateRepository(MetadataRepository metadataRepository,
        ProjectMetadataRepository projectMetadataRepository, NpgsqlDataSource dataSource, ILogger<ProjectMetadataAggregateRepository> logger)
        : base(dataSource, logger)
    {
        _metadataRepository = metadataRepository;
        _projectMetadataRepository = projectMetadataRepository;
    }

    public override List<ProjectMetadataAggregate> GetAll()
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

    public override ProjectMetadataAggregate Insert(ProjectMetadataAggregate entity)
    {
        _projectMetadataRepository.Insert(entity.ToProjectMetadata());
        return entity;
    }

    public override void Update(ProjectMetadataAggregate entity)
    {
        _projectMetadataRepository.Update(entity.ToProjectMetadata());
    }

    public override ProjectMetadataAggregate GetByKey(ProjectMetadataIds key)
    {
        ProjectMetadata projectMetadata = _projectMetadataRepository.GetByKey(key) ?? throw new Exception();
        Metadata metadata = _metadataRepository.GetById(key.MetadataId) ?? throw new Exception();


        return ToAggregate(metadata, projectMetadata);
    }

    public override void DeleteByKey(ProjectMetadataIds key)
    {
        _projectMetadataRepository.DeleteByKey(key);
    }

    private ProjectMetadataAggregate ToAggregate(Metadata m, ProjectMetadata pm)
    {
        return new ProjectMetadataAggregate(pm.ProjectId, pm.MetadataId, pm.MetadataValue, m.MetadataKey, m.MetadataType, m.DisplayName, m.Description);
    }
}