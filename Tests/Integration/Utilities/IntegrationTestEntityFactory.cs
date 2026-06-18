using Domain.entities;

namespace Tests.Integration.Utilities;

public static class IntegrationTestEntityFactory
{
    public static Project CreateProject(
        int? id = null,
        string name = "Test Project",
        string path = @"C:\Projects\Test Project",
        DateOnly? eventDate = null,
        int? parentProjectId = null)
    {
        return new Project(
            name,
            path,
            eventDate ?? new DateOnly(2026, 6, 17),
            parentProjectId,
            id
            );
    }

    public static Image CreateImage(
        int? imageId = null,
        int projectId = 1,
        string fileName = "TestImage",
        string fileType = ".png",
        string relationalFilePath = @"Originals\TestImage.png")
    {
        return new Image(
            projectId,
            fileName,
            fileType,
            relationalFilePath,
            imageId
            );
    }

    public static Metadata CreateMetadata(
        string metadataKey = "event_type",
        string metadataType = "string",
        string displayName = "Event Type",
        string description = "The type of photography event.")
    {
        return new Metadata(
            metadataKey,
            metadataType,
            displayName,
            description);
    }

    public static ProjectMetadata CreateProjectMetadata(
        int projectId,
        string metadataKey = "event_type",
        string metadataValue = "Concert",
        string metadataType = "string",
        string displayName = "Event Type",
        string description = "The type of photography event.")
    {
        return new ProjectMetadata(
            projectId,
            metadataValue,
            metadataKey,
            metadataType,
            displayName,
            description);
    }

    public static ProjectMetadata CreateProjectMetadata(
        int projectId,
        Metadata metadata,
        string metadataValue = "Concert")
    {
        return new ProjectMetadata(
            projectId,
            metadata,
            metadataValue);
    }

    public static SelectionSession CreateSelectionSession(
        int? id = null,
        int projectId = 1,
        string name = "Test Selection",
        List<int>? imageIds = null)
    {
        return new SelectionSession(
            projectId,
            name,
            imageIds ?? [],
            id);
    }
}