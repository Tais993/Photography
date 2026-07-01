using Application.interfaces.infrastructure.repositories;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Infrastructure;

public class ImageMetadataRepositoryIntegrationTests : IntegrationTestBase
{
    [Test]
    public void Insert_ThenGetByKey_ReturnsImageMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IImageMetadataRepository imageMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IImageMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());
        Image image = imageRepository.Insert(CreateImage(projectId: (int) project.Id!));

        // Metadata setup
        const string metadataKey = "event_type";
        const string metadataType = "string";
        const string displayName = "Event Type";
        const string description = "The type of photography event.";

        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            metadataType: metadataType,
            displayName: displayName,
            description: description));

        // Setup
        const string metadataValue = "Concert";

        ImageMetadata imageMetadata = CreateImageMetadata(
            imageId: (int)image.Id!,
            metadata: metadata,
            metadataValue: metadataValue);

        // Execution
        imageMetadataRepository.Insert(imageMetadata);
        ImageMetadata? retrievedImageMetadata =
            imageMetadataRepository.GetByKey((int)image.Id!, metadataKey);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedImageMetadata, Is.Not.Null);

            Assert.That(retrievedImageMetadata!.ImageId, Is.EqualTo(image.Id));
            Assert.That(retrievedImageMetadata.MetadataKey, Is.EqualTo(metadataKey));
            Assert.That(retrievedImageMetadata.MetadataValue, Is.EqualTo(metadataValue));
            Assert.That(retrievedImageMetadata.MetadataType, Is.EqualTo(metadataType));
            Assert.That(retrievedImageMetadata.DisplayName, Is.EqualTo(displayName));
            Assert.That(retrievedImageMetadata.Description, Is.EqualTo(description));
        }
    }

    [Test]
    public void InsertMultiple_GetAllByImageId_ReturnsOnlyImageMetadataForImage()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IImageMetadataRepository imageMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IImageMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());
        Image firstImage = imageRepository.Insert(CreateImage(
            projectId: (int) project.Id,
            fileName: "first.jpg"));

        Image secondImage = imageRepository.Insert(CreateImage(
            projectId: (int) project.Id,
            fileName: "second.jpg"));

        // Metadata setup
        Metadata eventTypeMetadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: "event_type",
            displayName: "Event Type"));

        Metadata locationMetadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: "location",
            displayName: "Location"));

        // Setup
        ImageMetadata firstImageEventType = CreateImageMetadata(
            imageId: (int)firstImage.Id!,
            metadata: eventTypeMetadata,
            metadataValue: "Concert");

        ImageMetadata firstImageLocation = CreateImageMetadata(
            imageId: (int)firstImage.Id!,
            metadata: locationMetadata,
            metadataValue: "Uden");

        ImageMetadata secondImageEventType = CreateImageMetadata(
            imageId: (int)secondImage.Id!,
            metadata: eventTypeMetadata,
            metadataValue: "Travel");

        // Execution
        imageMetadataRepository.Insert(firstImageEventType);
        imageMetadataRepository.Insert(firstImageLocation);
        imageMetadataRepository.Insert(secondImageEventType);

        List<ImageMetadata> imageMetadata =
            imageMetadataRepository.GetAllByImageId((int)firstImage.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageMetadata, Has.Count.EqualTo(2));

            Assert.That(imageMetadata.All(metadata => metadata.ImageId == firstImage.Id), Is.True);
            Assert.That(imageMetadata.Select(metadata => metadata.MetadataKey), Does.Contain("event_type"));
            Assert.That(imageMetadata.Select(metadata => metadata.MetadataKey), Does.Contain("location"));
        }
    }

    [Test]
    public void Update_ThenGetByKey_ReturnsUpdatedImageMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IImageMetadataRepository imageMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IImageMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());
        Image image = imageRepository.Insert(CreateImage(projectId: (int) project.Id));

        // Metadata setup
        const string metadataKey = "event_type";
        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            displayName: "Event Type"));

        // Setup
        const string originalMetadataValue = "Concert";
        const string updatedMetadataValue = "Festival";

        ImageMetadata imageMetadata = CreateImageMetadata(
            imageId: (int)image.Id!,
            metadata: metadata,
            metadataValue: originalMetadataValue);

        // Execution
        imageMetadataRepository.Insert(imageMetadata);

        ImageMetadata updatedImageMetadata = CreateImageMetadata(
            imageId: (int)image.Id!,
            metadata: metadata,
            metadataValue: updatedMetadataValue);

        imageMetadataRepository.Update(updatedImageMetadata);

        ImageMetadata? retrievedImageMetadata =
            imageMetadataRepository.GetByKey((int)image.Id!, metadataKey);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedImageMetadata, Is.Not.Null);

            Assert.That(retrievedImageMetadata!.ImageId, Is.EqualTo(image.Id));
            Assert.That(retrievedImageMetadata.MetadataKey, Is.EqualTo(metadataKey));
            Assert.That(retrievedImageMetadata.MetadataValue, Is.EqualTo(updatedMetadataValue));
        }
    }

    [Test]
    public void DeleteByKey_RemovesImageMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IImageMetadataRepository imageMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IImageMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());
        Image image = imageRepository.Insert(CreateImage(projectId: (int) project.Id));

        // Metadata setup
        const string metadataKey = "event_type";
        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            displayName: "Event Type"));

        // Setup
        ImageMetadata imageMetadata = CreateImageMetadata(
            imageId: (int)image.Id!,
            metadata: metadata,
            metadataValue: "Concert");

        // Execution
        imageMetadataRepository.Insert(imageMetadata);
        imageMetadataRepository.DeleteByKey((int)image.Id!, metadataKey);

        ImageMetadata? retrievedImageMetadata =
            imageMetadataRepository.GetByKey((int)image.Id!, metadataKey);

        List<ImageMetadata> imageMetadataList =
            imageMetadataRepository.GetAllByImageId((int)image.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedImageMetadata, Is.Null);
            Assert.That(imageMetadataList, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void DeleteImage_CascadesImageMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IImageMetadataRepository imageMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IImageMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());
        Image image = imageRepository.Insert(CreateImage(projectId: (int) project.Id));

        // Metadata setup
        const string metadataKey = "event_type";
        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            displayName: "Event Type"));

        // Setup
        ImageMetadata imageMetadata = CreateImageMetadata(
            imageId: (int)image.Id!,
            metadata: metadata,
            metadataValue: "Concert");

        // Execution
        imageMetadataRepository.Insert(imageMetadata);
        imageRepository.DeleteById((int)image.Id!);

        ImageMetadata? retrievedMetadata =
            imageMetadataRepository.GetByKey((int)image.Id, metadata.MetadataKey!);

        List<ImageMetadata> metadatas =
            imageMetadataRepository.GetAllByImageId((int)image.Id);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedMetadata, Is.Null);
            Assert.That(metadatas, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void DeleteMetadata_CascadesImageMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IImageMetadataRepository imageMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IImageMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());
        Image image = imageRepository.Insert(CreateImage(projectId: (int) project.Id));

        // Metadata setup
        const string metadataKey = "event_type";
        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            displayName: "Event Type"));

        // Setup
        ImageMetadata imageMetadata = CreateImageMetadata(
            imageId: (int)image.Id!,
            metadata: metadata,
            metadataValue: "Concert");

        // Execution
        imageMetadataRepository.Insert(imageMetadata);
        metadataRepository.DeleteByKey(metadataKey);

        ImageMetadata? retrievedMetadata =
            imageMetadataRepository.GetByKey((int)image.Id!, metadataKey);

        List<ImageMetadata> metadatas =
            imageMetadataRepository.GetAllByImageId((int)image.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedMetadata, Is.Null);
            Assert.That(metadatas, Has.Count.EqualTo(0));
        }
    }
}