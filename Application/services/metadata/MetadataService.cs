using Application.interfaces.infrastructure;
using Application.interfaces.services.metadata;
using Domain.entities;
using Microsoft.Extensions.Logging;

namespace Application.services.metadata;

public class MetadataService : IMetadataService
{
    private readonly IMetadataRepository _metadataRepository;
    private readonly ILogger<MetadataService> _logger;

    public MetadataService(
        IMetadataRepository metadataRepository,
        ILogger<MetadataService> logger)
    {
        _metadataRepository = metadataRepository;
        _logger = logger;
    }

    public Metadata CreateMetadata(string metadataKey, string metadataType, string displayName, string description)
    {
        return CreateMetadata(new Metadata(metadataKey, metadataType, displayName, description));
    }

    public Metadata CreateMetadata(Metadata metadata)
    {
        _logger.LogInformation("Creating metadata: {MetadataKey}", metadata.MetadataKey);
        return _metadataRepository.Insert(metadata);
    }

    public void UpdateMetadata(string metadataKey, string metadataType, string displayName, string description)
    {
        UpdateMetadata(new Metadata(metadataKey, metadataType, displayName, description));
    }

    public void UpdateMetadata(Metadata metadata)
    {
        _logger.LogInformation("Updating metadata: {MetadataKey}", metadata.MetadataKey);
        _metadataRepository.Update(metadata);
    }

    public Metadata? GetMetadata(Metadata metadata)
    {
        if (metadata.MetadataKey == null)
        {
            _logger.LogWarning("Could not get metadata because metadata key was null");
            throw new ArgumentNullException("metadata.MetadataKey");
        }

        return GetMetadata(metadata.MetadataKey);
    }

    public Metadata? GetMetadata(string metadataKey)
    {
        _logger.LogDebug("Getting metadata: {MetadataKey}", metadataKey);
        return _metadataRepository.GetByKey(metadataKey);
    }

    public void DeleteMetadata(Metadata metadata)
    {
        if (metadata.MetadataKey == null)
        {
            _logger.LogWarning("Could not delete metadata because metadata key was null");
            throw new ArgumentNullException("metadata.MetadataKey");
        }

        DeleteMetadata(metadata.MetadataKey);
    }

    public void DeleteMetadata(string metadataKey)
    {
        _logger.LogInformation("Deleting metadata: {MetadataKey}", metadataKey);
        _metadataRepository.DeleteByKey(metadataKey);
    }
}