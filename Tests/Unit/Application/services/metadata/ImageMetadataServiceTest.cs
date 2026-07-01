using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.services.metadata;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services.metadata;

[TestFixture]
[TestOf(typeof(ImageMetadataService))]
public class ImageMetadataServiceTest
{
    private Mock<IImageMetadataRepository> _imageMetadataRepository = null!;
    private ImageMetadataService _imageMetadataService = null!;
    private Mock<ILogger<ImageMetadataService>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _imageMetadataRepository = new Mock<IImageMetadataRepository>();
        _logger = new Mock<ILogger<ImageMetadataService>>();

        _imageMetadataService = new ImageMetadataService(
            _imageMetadataRepository.Object,
            _logger.Object
        );
    }

    [Test]
    public void AddMetadataToImage_WithNullValue_InsertsImageMetadataWithEmptyValue()
    {
        // Execution
        _imageMetadataService.AddMetadataToImage(1, "location", null);

        // Asserts
        _imageMetadataRepository.Verify(r => r.Insert(
            It.Is<ImageMetadata>(imageMetadata =>
                imageMetadata.ImageId == 1 &&
                imageMetadata.MetadataKey == "location" &&
                imageMetadata.MetadataValue == string.Empty
            )
        ), Times.Once);
    }

    [Test]
    public void UpdateMetadataForImage_WithNullValue_UpdatesImageMetadataWithEmptyValue()
    {
        // Execution
        _imageMetadataService.UpdateMetadataForImage(1, "location", null);

        // Asserts
        _imageMetadataRepository.Verify(r => r.Update(
            It.Is<ImageMetadata>(imageMetadata =>
                imageMetadata.ImageId == 1 &&
                imageMetadata.MetadataKey == "location" &&
                imageMetadata.MetadataValue == string.Empty
            )
        ), Times.Once);
    }
}