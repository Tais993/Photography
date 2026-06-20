using Application.interfaces.services;
using static Application.Constants;

namespace Application.services;

public class MetadataInitialisationService : IMetadataInitialisationService
{
    private readonly IProjectMetadataService _projectMetadataService;

    public MetadataInitialisationService(IProjectMetadataService projectMetadataService)
    {
        _projectMetadataService = projectMetadataService;
    }

    public void EnsureRequiredMetadataExists()
    {
        EnsureMetadataExists(
            OriginalsFolderMetadataKey,
            "Folder",
            "Originals folder",
            "The mapped originals folder for this project.");

        EnsureMetadataExists(
            EditingFolderMetadataKey,
            "Folder",
            "Editing folder",
            "The mapped editing folder for this project.");

        EnsureMetadataExists(
            FinalsFolderMetadataKey,
            "Folder",
            "Finals folder",
            "The mapped finals folder for this project.");
    }

    private void EnsureMetadataExists(string metadataKey, string metadataType, string displayName, string description)
    {
        try
        {
            _projectMetadataService.GetMetadata(metadataKey);
        }
        catch
        {
            _projectMetadataService.CreateMetadata(
                metadataKey,
                metadataType,
                displayName,
                description);
        }
    }
}