using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.project;
using Application.services.project;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services.project;

[TestFixture]
[TestOf(typeof(ProjectResolverService))]
public class ProjectResolverServiceTest
{
    private Mock<IProjectRepository> _projectRepository = null!;
    private Mock<IProjectInfoFileService> _projectInfoFileService = null!;
    private Mock<ILogger<ProjectResolverService>> _logger = null!;
    private ProjectResolverService _projectResolverService = null!;

    [SetUp]
    public void SetUp()
    {
        _projectRepository = new Mock<IProjectRepository>();
        _projectInfoFileService = new Mock<IProjectInfoFileService>();
        _logger = new Mock<ILogger<ProjectResolverService>>();

        _projectResolverService = new ProjectResolverService(
            _projectRepository.Object,
            _projectInfoFileService.Object,
            _logger.Object
        );
    }

    [Test]
    public void ResolveProject_ProjectExists_ReturnsProject_WithEmptyProjectId()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");

        Project expectedProject = new Project(
            "Merijn",
            projectDirectory,
            new DateOnly(2024, 7, 4),
            2
        );

        // Mocks
        _projectInfoFileService
            .Setup(s => s.ResolveProjectId(projectDirectory))
            .Returns(2);

        _projectRepository
            .Setup(r => r.GetById(2))
            .Returns(expectedProject);

        // Execution
        Project? result = _projectResolverService.ResolveProject(projectDirectory, 0);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProject));
        _projectInfoFileService.Verify(s => s.ResolveProjectId(projectDirectory), Times.Once);
        _projectRepository.Verify(r => r.GetById(2), Times.Once);
    }

    [Test]
    public void ResolveProject_ProjectExists_ReturnsProject_WithProjectId()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");

        Project expectedProject = new Project(
            "Merijn",
            projectDirectory,
            new DateOnly(2024, 7, 4),
            2
        );

        // Mocks
        _projectRepository
            .Setup(r => r.GetById(2))
            .Returns(expectedProject);

        // Execution
        Project? result = _projectResolverService.ResolveProject(projectDirectory, 2);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProject));
        _projectRepository.Verify(r => r.GetById(2), Times.Once);
        _projectInfoFileService.Verify(s => s.ResolveProjectId(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ResolveProject_ProjectNotInitialised_ReturnsNull()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");

        // Mocks
        _projectInfoFileService
            .Setup(s => s.ResolveProjectId(projectDirectory))
            .Returns((int?)null);

        // Execution
        Project? result = _projectResolverService.ResolveProject(projectDirectory);

        // Asserts
        Assert.That(result, Is.Null);
        _projectInfoFileService.Verify(s => s.ResolveProjectId(projectDirectory), Times.Once);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ResolveProjectId_ProjectExists_ReturnsProjectId_WithEmptyProjectId()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");

        const int projectId = 6;

        // Mocks
        _projectInfoFileService
            .Setup(s => s.ResolveProjectId(projectDirectory))
            .Returns(projectId);

        // Execution
        int result = _projectResolverService.ResolveProjectId(projectDirectory, 0);

        // Asserts
        Assert.That(result, Is.EqualTo(projectId));
        _projectInfoFileService.Verify(s => s.ResolveProjectId(projectDirectory), Times.Once);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ResolveProjectId_ProjectIdWasGiven_ReturnsProjectId()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");

        const int projectId = 6;

        // Execution
        int result = _projectResolverService.ResolveProjectId(projectDirectory, projectId);

        // Asserts
        Assert.That(result, Is.EqualTo(projectId));
        _projectInfoFileService.Verify(s => s.ResolveProjectId(It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ResolveProjectId_ProjectNotInitialised_ReturnsZero()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");

        // Mocks
        _projectInfoFileService
            .Setup(s => s.ResolveProjectId(projectDirectory))
            .Returns((int?)null);

        // Execution
        int result = _projectResolverService.ResolveProjectId(projectDirectory);

        // Asserts
        Assert.That(result, Is.EqualTo(0));
        _projectInfoFileService.Verify(s => s.ResolveProjectId(projectDirectory), Times.Once);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }
}