using Application.interfaces.services;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services;

public class MetadataInitialisationService : IMetadataInitialisationService
{
    private readonly IProjectMetadataService _projectMetadataService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MetadataInitialisationService> _logger;
    private readonly ICollectionMetadataService _collectionMetadataService;

    public MetadataInitialisationService(IProjectMetadataService projectMetadataService, IConfiguration configuration, ILogger<MetadataInitialisationService> logger, ICollectionMetadataService collectionMetadataService)
    {
        _projectMetadataService = projectMetadataService;
        _configuration = configuration;
        _logger = logger;
        _collectionMetadataService = collectionMetadataService;
    }

    public void EnsureRequiredMetadataExists()
    {
        EnsureFolderMetadataExists();
        EnsureCollectionMetadataExists();
    }

    private void EnsureFolderMetadataExists()
    {
        EnsureMetadataExists(
            OriginalsFolderMetadataKey,
            FolderMetadataType,
            "Originals folder",
            "The mapped originals folder for this project.");

        EnsureMetadataExists(
            EditingFolderMetadataKey,
            FolderMetadataType,
            "Editing folder",
            "The mapped editing folder for this project.");

        EnsureMetadataExists(
            FinalsFolderMetadataKey,
            FolderMetadataType,
            "Finals folder",
            "The mapped finals folder for this project.");
    }

    private void EnsureCollectionMetadataExists()
    {
        Dictionary<string, CollectionMetadataConfiguration> collectionConfigurations = _collectionMetadataService.GetCollectionConfigurations();

        foreach ((string collectionName, CollectionMetadataConfiguration collectionConfiguration) in collectionConfigurations)
        {
            if (string.IsNullOrWhiteSpace(collectionConfiguration.MetadataKey))
            {
                _logger.LogWarning("Skipping collection metadata for {CollectionName}, metadata key is empty", collectionName);
                continue;
            }

            EnsureMetadataExists(
                collectionConfiguration.MetadataKey,
                CollectionMetadataType,
                collectionConfiguration.DisplayName,
                collectionConfiguration.Description);
        }
    }

    private void EnsureMetadataExists(string metadataKey, string metadataType, string displayName, string description)
    {
        if (_projectMetadataService.GetMetadata(metadataKey) is not null)
        {
            _logger.LogDebug("Metadata already exists: {MetadataKey}", metadataKey);
            return;
        }

        _logger.LogInformation("Creating metadata: {MetadataKey}", metadataKey);

        _projectMetadataService.CreateMetadata(
            metadataKey,
            metadataType,
            displayName,
            description);
    }
}