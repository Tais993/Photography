using Application;
using Application.interfaces.infrastructure;
using Application.services.project;
using Domain.entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services;

[TestFixture]
[TestOf(typeof(ProjectResolverService))]
public class ProjectResolverServiceTest
{
    private Mock<IProjectRepository> _projectRepository = null!;
    private Mock<IFiles> _files = null!;
    private Mock<ILogger<ProjectService>> _logger = null!;
    private ProjectResolverService _projectResolverService = null!;

    [SetUp]
    public void SetUp()
    {
        _projectRepository = new Mock<IProjectRepository>();
        _files = new Mock<IFiles>();
        _logger = new Mock<ILogger<ProjectService>>();

        _projectResolverService = new ProjectResolverService(
            _projectRepository.Object,
            _files.Object,
            _logger.Object
        );
    }

    [Test]
    public void ResolveProject_ProjectExists_ReturnsProject_WithEmptyProjectId()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");
        string projectInfoPath = Path.Combine(projectDirectory, Constants.ProjectInfoFile);

        Project expectedProject = new Project(
            "Merijn",
            projectDirectory,
            new DateOnly(2024, 7, 4),
            2
        );

        // Mocks
        _files
            .Setup(f => f.GetFullPath(projectDirectory))
            .Returns(projectDirectory);

        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files
            .Setup(f => f.Exists(projectInfoPath))
            .Returns(true);

        _files
            .Setup(f => f.ReadAllText(projectInfoPath))
            .Returns("2");

        _projectRepository
            .Setup(r => r.GetById(2))
            .Returns(expectedProject);

        // Execution
        Project? result = _projectResolverService.ResolveProject(projectDirectory, 0);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProject));
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
        _files.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void ResolveProject_ProjectNotInitialised_ReturnsNull()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");
        string projectInfoPath = Path.Combine(projectDirectory, Constants.ProjectInfoFile);

        // Mocks
        _files
            .Setup(f => f.GetFullPath(projectDirectory))
            .Returns(projectDirectory);

        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files
            .Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files
            .Setup(f => f.GetParentDirectory(projectDirectory))
            .Returns((DirectoryInfo?)null);

        // Execution
        Project? result = _projectResolverService.ResolveProject(projectDirectory);

        // Asserts
        Assert.That(result, Is.Null);
        _files.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ResolveProjectId_ProjectExists_ReturnsProjectId_WithEmptyProjectId()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");
        string projectInfoPath = Path.Combine(projectDirectory, Constants.ProjectInfoFile);

        const int projectId = 6;

        // Mocks
        _files
            .Setup(f => f.GetFullPath(projectDirectory))
            .Returns(projectDirectory);

        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files
            .Setup(f => f.Exists(projectInfoPath))
            .Returns(true);

        _files
            .Setup(f => f.ReadAllText(projectInfoPath))
            .Returns(projectId + "");

        // Execution
        int result = _projectResolverService.ResolveProjectId(projectDirectory, 0);

        // Asserts
        Assert.That(result, Is.EqualTo(projectId));
        _files.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Once);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ResolveProjectId_ProjectExistsInParentDirectory_ReturnsProjectId()
    {
        string parentDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");
        string projectDirectory = Path.Combine(parentDirectory, "Editing");

        string childProjectInfoPath = Path.Combine(projectDirectory, Constants.ProjectInfoFile);
        string parentProjectInfoPath = Path.Combine(parentDirectory, Constants.ProjectInfoFile);

        const int projectId = 6;

        // Mocks
        _files
            .Setup(f => f.GetFullPath(projectDirectory))
            .Returns(projectDirectory);

        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(childProjectInfoPath);

        _files
            .Setup(f => f.Exists(childProjectInfoPath))
            .Returns(false);

        _files
            .Setup(f => f.GetParentDirectory(projectDirectory))
            .Returns(new DirectoryInfo(parentDirectory));

        _files
            .Setup(f => f.Combine(parentDirectory, Constants.ProjectInfoFile))
            .Returns(parentProjectInfoPath);

        _files
            .Setup(f => f.Exists(parentProjectInfoPath))
            .Returns(true);

        _files
            .Setup(f => f.ReadAllText(parentProjectInfoPath))
            .Returns(projectId + "");

        // Execution
        int result = _projectResolverService.ResolveProjectId(projectDirectory, 0);

        // Asserts
        Assert.That(result, Is.EqualTo(projectId));
        _files.Verify(f => f.ReadAllText(parentProjectInfoPath), Times.Once);
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
        _files.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ResolveProjectId_ProjectNotInitialised_ReturnsZero()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");
        string projectInfoPath = Path.Combine(projectDirectory, Constants.ProjectInfoFile);

        // Mocks
        _files
            .Setup(f => f.GetFullPath(projectDirectory))
            .Returns(projectDirectory);

        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files
            .Setup(f => f.Exists(projectInfoPath))
            .Returns(false);

        _files
            .Setup(f => f.GetParentDirectory(projectDirectory))
            .Returns((DirectoryInfo?)null);

        // Execution
        int result = _projectResolverService.ResolveProjectId(projectDirectory);

        // Asserts
        Assert.That(result, Is.EqualTo(0));
        _files.Verify(f => f.ReadAllText(It.IsAny<string>()), Times.Never);
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ResolveProjectId_ProjectInfoContentIsInvalid_ReturnsZero()
    {
        string projectDirectory = Path.Combine(Path.GetTempPath(), "2024-07-04-Merijn");
        string projectInfoPath = Path.Combine(projectDirectory, Constants.ProjectInfoFile);

        // Mocks
        _files
            .Setup(f => f.GetFullPath(projectDirectory))
            .Returns(projectDirectory);

        _files
            .Setup(f => f.Combine(projectDirectory, Constants.ProjectInfoFile))
            .Returns(projectInfoPath);

        _files
            .Setup(f => f.Exists(projectInfoPath))
            .Returns(true);

        _files
            .Setup(f => f.ReadAllText(projectInfoPath))
            .Returns("invalid");

        // Execution
        int result = _projectResolverService.ResolveProjectId(projectDirectory);

        // Asserts
        Assert.That(result, Is.EqualTo(0));
        _projectRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }
}