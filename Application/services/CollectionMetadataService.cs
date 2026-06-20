using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Application.Constants;

namespace Application.services;

public class CollectionMetadataService : ICollectionMetadataService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CollectionMetadataService> _logger;
    private readonly IFiles _files;



    public CollectionMetadataService(IConfiguration configuration, ILogger<CollectionMetadataService> logger, IFiles files)
    {
        _configuration = configuration;
        _logger = logger;
        _files = files;
    }

    public Dictionary<string, CollectionMetadataConfiguration> GetCollectionConfigurations()
    {
        return _configuration
            .GetSection(CollectionsConfigKey)
            .GetChildren()
            .ToDictionary(
                section => section.Key,
                section => section.Get<CollectionMetadataConfiguration>() ?? new CollectionMetadataConfiguration());
    }

    public CollectionMetadataConfiguration? GetCollectionMetadataConfiguration(string collectionDirectory)
    {
        string collectionFolderName = _files.GetPathEnd(collectionDirectory);
        Dictionary<string, CollectionMetadataConfiguration> collectionConfigurations = GetCollectionConfigurations();

        foreach ((string collectionName, CollectionMetadataConfiguration collectionConfiguration) in collectionConfigurations)
        {
            string[] matchingFolderNames = CompareFolderNames(
                [collectionFolderName],
                collectionConfiguration.FolderNames);

            if (matchingFolderNames.Length == 1)
            {
                return collectionConfiguration;
            }

            if (string.Equals(collectionName, collectionFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return collectionConfiguration;
            }
        }

        _logger.LogDebug("No collection metadata configuration found for collection folder: {CollectionFolder}", collectionFolderName);
        return null;
    }

    private static string[] CompareFolderNames(string[] folderNames, string[] possibleFolderNames)
    {
        return folderNames
            .Where(folderName => possibleFolderNames.Contains(folderName, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}