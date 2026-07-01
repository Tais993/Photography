using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.services.metadata;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services.metadata;

[TestFixture]
[TestOf(typeof(MetadataService))]
public class MetadataServiceTest
{
    private Mock<IMetadataRepository> _metadataRepository = null!;
    private MetadataService _metadataService = null!;
    private Mock<ILogger<MetadataService>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _metadataRepository = new Mock<IMetadataRepository>();
        _logger = new Mock<ILogger<MetadataService>>();

        _metadataService = new MetadataService(
            _metadataRepository.Object,
            _logger.Object
        );
    }

    [Test]
    public void CreateMetadata_WithValues_InsertsMetadata()
    {
        Metadata expectedMetadata = new(
            "location",
            "text",
            "Location",
            "The location of the project"
        );

        // Mocks
        _metadataRepository
            .Setup(r => r.Insert(It.IsAny<Metadata>()))
            .Returns(expectedMetadata);

        // Execution
        Metadata result = _metadataService.CreateMetadata(
            "location",
            "text",
            "Location",
            "The location of the project"
        );

        // Asserts
        Assert.That(result, Is.EqualTo(expectedMetadata));

        _metadataRepository.Verify(r => r.Insert(
            It.Is<Metadata>(metadata =>
                metadata.MetadataKey == "location" &&
                metadata.MetadataType == "text" &&
                metadata.DisplayName == "Location" &&
                metadata.Description == "The location of the project"
            )
        ), Times.Once);
    }

    [Test]
    public void GetMetadata_WithMetadataWithoutKey_ThrowsArgumentNullException()
    {
        Metadata metadata = new(
            null,
            "text",
            "Location",
            "The location of the project"
        );

        // Execution & Asserts
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            _metadataService.GetMetadata(metadata)
        )!;

        Assert.That(exception.ParamName, Is.EqualTo("metadata.MetadataKey"));
    }

    [Test]
    public void DeleteMetadata_WithMetadataWithoutKey_ThrowsArgumentNullException()
    {
        Metadata metadata = new(
            null,
            "text",
            "Location",
            "The location of the project"
        );

        // Execution & Asserts
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            _metadataService.DeleteMetadata(metadata)
        )!;

        Assert.That(exception.ParamName, Is.EqualTo("metadata.MetadataKey"));
    }
}