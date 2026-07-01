using Application.interfaces.infrastructure.repositories;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Infrastructure;

public class MetadataRepositoryIntegrationTests : IntegrationTestBase
{
    [Test]
    public void Insert_ThenGetByKey_ReturnsMetadata()
    {
        using IServiceScope scope = CreateScope();

        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();

        // Setup
        const string metadataKey = "event_type";
        const string metadataType = "string";
        const string displayName = "Event Type";
        const string description = "The type of photography event.";

        Metadata metadata = CreateMetadata(
            metadataKey: metadataKey,
            metadataType: metadataType,
            displayName: displayName,
            description: description);

        // Execution
        metadataRepository.Insert(metadata);
        Metadata retrievedMetadata = metadataRepository.GetByKey(metadataKey);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedMetadata, Is.Not.Null);

            Assert.That(retrievedMetadata.MetadataKey, Is.EqualTo(metadataKey));
            Assert.That(retrievedMetadata.MetadataType, Is.EqualTo(metadataType));
            Assert.That(retrievedMetadata.DisplayName, Is.EqualTo(displayName));
            Assert.That(retrievedMetadata.Description, Is.EqualTo(description));
        }
    }

    [Test]
    public void InsertMultiple_GetAll_ReturnsAllMetadata()
    {
        using IServiceScope scope = CreateScope();

        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();

        // Setup

        const string firstMetadataKey = "event_type";
        Metadata firstMetadata = CreateMetadata(
            metadataKey: firstMetadataKey);

        const string secondMetadataKey = "event_types";
        Metadata secondMetadata = CreateMetadata(
            metadataKey: secondMetadataKey);

        // Execution
        metadataRepository.Insert(firstMetadata);
        metadataRepository.Insert(secondMetadata);
        List<Metadata> metadatas = metadataRepository.GetAll();

        // Asserts
        Assert.That(metadatas, Has.Count.EqualTo(2));
    }

    [Test]
    public void Update_ThenGetByKey_ReturnsUpdatedMetadata()
    {
        using IServiceScope scope = CreateScope();

        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();

        // Setup
        const string metadataKey = "event_type";
        const string displayName = "Event Type";

        Metadata metadata = CreateMetadata(
            metadataKey: metadataKey,
            displayName: displayName);

        const string updatedDisplayName = "Not the event type";

        Metadata updatedMetadata = CreateMetadata(
            metadataKey: metadataKey,
            displayName: updatedDisplayName);

        // Execution
        Metadata insertedMetadata = metadataRepository.Insert(metadata);
        metadataRepository.Update(updatedMetadata);

        Metadata retrievedMetadata = metadataRepository.GetByKey(metadataKey);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(insertedMetadata, Is.Not.Null);
            Assert.That(retrievedMetadata, Is.Not.Null);

            Assert.That(retrievedMetadata.MetadataKey, Is.EqualTo(metadataKey));

            Assert.That(insertedMetadata.DisplayName, Is.EqualTo(displayName));
            Assert.That(retrievedMetadata.DisplayName, Is.EqualTo(updatedDisplayName));
        }
    }

    [Test]
    public void DeleteByKey_RemovesMetadata()
    {
        using IServiceScope scope = CreateScope();

        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();

        // Setup
        const string metadataKey = "event_type";
        const string metadataType = "string";
        const string displayName = "Event Type";
        const string description = "The type of photography event.";

        Metadata metadata = CreateMetadata(
            metadataKey: metadataKey,
            metadataType: metadataType,
            displayName: displayName,
            description: description);

        // Execution
        metadataRepository.Insert(metadata);
        metadataRepository.DeleteByKey(metadataKey);

        Metadata? retrievedMetadata = metadataRepository.GetByKey(metadata.MetadataKey!);
        List<Metadata> metadatas = metadataRepository.GetAll();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedMetadata, Is.Null);
            Assert.That(metadatas, Has.Count.EqualTo(0));
        }
    }
}