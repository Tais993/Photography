using Application.interfaces.infrastructure.repositories;
using Application.services.metadata;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services.metadata;

[TestFixture]
[TestOf(typeof(ProjectMetadataService))]
public class ProjectMetadataServiceTest
{
    private Mock<IProjectMetadataRepository> _projectMetadataRepository = null!;
    private ProjectMetadataService _projectMetadataService = null!;
    private Mock<ILogger<ProjectMetadataService>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _projectMetadataRepository = new Mock<IProjectMetadataRepository>();
        _logger = new Mock<ILogger<ProjectMetadataService>>();

        _projectMetadataService = new ProjectMetadataService(
            _projectMetadataRepository.Object,
            _logger.Object
        );
    }

    [Test]
    public void GetProjectMetadata_WithProjectWithoutId_ThrowsArgumentNullException()
    {
        Project project = new(
            "Holiday",
            @"C:\Photos\Holiday",
            new DateOnly(2024, 7, 4),
            null,
            null
        );

        // Execution & Asserts
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            _projectMetadataService.GetProjectMetadata(project)
        )!;

        Assert.That(exception.ParamName, Is.EqualTo("project.Id"));
    }

    [Test]
    public void GetProjectMetadata_WithProjectAndMetadataKeyWithoutProjectId_ThrowsArgumentNullException()
    {
        Project project = new(
            "Holiday",
            @"C:\Photos\Holiday",
            new DateOnly(2024, 7, 4),
            null,
            null
        );

        // Execution & Asserts
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            _projectMetadataService.GetProjectMetadata(project, "location")
        )!;

        Assert.That(exception.ParamName, Is.EqualTo("project.Id"));
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
}