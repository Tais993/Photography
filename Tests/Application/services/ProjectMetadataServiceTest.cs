using Application.interfaces;
using Application.services;
using Domain.entities;
using Moq;

namespace Tests.Application.services;

[TestFixture]
[TestOf(typeof(ProjectMetadataService))]
public class ProjectMetadataServiceTest
{
    private Mock<IMetadataRepository> _metadataRepository = null!;
    private Mock<IProjectMetadataRepository> _projectMetadataRepository = null!;
    private ProjectMetadataService _projectMetadataService = null!;

    [SetUp]
    public void SetUp()
    {
        _metadataRepository = new Mock<IMetadataRepository>();
        _projectMetadataRepository = new Mock<IProjectMetadataRepository>();

        _projectMetadataService = new ProjectMetadataService(
            _projectMetadataRepository.Object,
            _metadataRepository.Object
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
        Metadata result = _projectMetadataService.CreateMetadata(
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
    public void UpdateMetadata_WithMetadata_UpdatesMetadata()
    {
        Metadata metadata = new(
            "location",
            "text",
            "Location",
            "The location of the project"
        );

        // Execution
        _projectMetadataService.UpdateMetadata(metadata);

        // Asserts
        _metadataRepository.Verify(r => r.Update(metadata), Times.Once);
    }

    [Test]
    public void GetMetadata_WithMetadata_GetsMetadataByKey()
    {
        Metadata metadata = new(
            "location",
            "text",
            "Location",
            "The location of the project"
        );

        // Mocks
        _metadataRepository
            .Setup(r => r.GetByKey("location"))
            .Returns(metadata);

        // Execution
        Metadata? result = _projectMetadataService.GetMetadata(metadata);

        // Asserts
        Assert.That(result, Is.EqualTo(metadata));
        _metadataRepository.Verify(r => r.GetByKey("location"), Times.Once);
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
            _projectMetadataService.GetMetadata(metadata)
        )!;

        Assert.That(exception.ParamName, Is.EqualTo("metadata.MetadataKey"));
    }

    [Test]
    public void DeleteMetadata_WithMetadata_DeletesMetadataByKey()
    {
        Metadata metadata = new(
            "location",
            "text",
            "Location",
            "The location of the project"
        );

        // Execution
        _projectMetadataService.DeleteMetadata(metadata);

        // Asserts
        _metadataRepository.Verify(r => r.DeleteById("location"), Times.Once);
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
            _projectMetadataService.DeleteMetadata(metadata)
        )!;

        Assert.That(exception.ParamName, Is.EqualTo("metadata.MetadataKey"));
    }

    [Test]
    public void GetProjectMetadata_WithProject_GetsMetadataByProjectId()
    {
        Project project = new(
            1,
            "Holiday",
            @"C:\Photos\Holiday",
            new DateOnly(2024, 7, 4),
            null
        );

        List<ProjectMetadata> expectedProjectMetadata =
        [
            new ProjectMetadata(1, "location", "Uden")
        ];

        // Mocks
        _projectMetadataRepository
            .Setup(r => r.GetAllByProjectId(1))
            .Returns(expectedProjectMetadata);

        // Execution
        List<ProjectMetadata> result = _projectMetadataService.GetProjectMetadata(project);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProjectMetadata));
        _projectMetadataRepository.Verify(r => r.GetAllByProjectId(1), Times.Once);
    }

    [Test]
    public void GetProjectMetadata_WithProjectWithoutId_ThrowsArgumentNullException()
    {
        Project project = new(
            null,
            "Holiday",
            @"C:\Photos\Holiday",
            new DateOnly(2024, 7, 4),
            null
        );

        // Execution & Asserts
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            _projectMetadataService.GetProjectMetadata(project)
        )!;

        Assert.That(exception.ParamName, Is.EqualTo("project.Id"));
    }

    [Test]
    public void AddMetadataToProject_WithValue_InsertsProjectMetadata()
    {
        // Execution
        _projectMetadataService.AddMetadataToProject(1, "location", "Uden");

        // Asserts
        _projectMetadataRepository.Verify(r => r.Insert(
            It.Is<ProjectMetadata>(projectMetadata =>
                projectMetadata.ProjectId == 1 &&
                projectMetadata.MetadataKey == "location" &&
                projectMetadata.MetadataValue == "Uden"
            )
        ), Times.Once);
    }

    [Test]
    public void AddMetadataToProject_WithNullValue_InsertsProjectMetadataWithEmptyValue()
    {
        // Execution
        _projectMetadataService.AddMetadataToProject(1, "location", null);

        // Asserts
        _projectMetadataRepository.Verify(r => r.Insert(
            It.Is<ProjectMetadata>(projectMetadata =>
                projectMetadata.ProjectId == 1 &&
                projectMetadata.MetadataKey == "location" &&
                projectMetadata.MetadataValue == string.Empty
            )
        ), Times.Once);
    }

    [Test]
    public void RemoveMetadataFromProject_DeletesProjectMetadataByKey()
    {
        // Execution
        _projectMetadataService.RemoveMetadataFromProject(1, "location");

        // Asserts
        _projectMetadataRepository.Verify(r => r.DeleteByKey(1, "location"), Times.Once);
    }
}
