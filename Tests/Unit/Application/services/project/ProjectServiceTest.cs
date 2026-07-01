using Application;
using Application.interfaces.infrastructure;
using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.project;
using Application.services.project;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services.project;

[TestFixture]
[TestOf(typeof(ProjectService))]
public class ProjectServiceTest
{
    private Mock<IProjectRepository> _projectRepository = null!;
    private Mock<ILogger<ProjectService>> _logger = null!;
    private Mock<IFiles> _files = null!;
    private Mock<IProjectFolderService> _projectFolderService = null!;
    private Mock<IProjectInfoFileService> _projectInfoFileService = null!;
    private IConfiguration _configuration = null!;

    private ProjectService _projectService = null!;

    [SetUp]
    public void SetUp()
    {
        _projectRepository = new Mock<IProjectRepository>();
        _logger = new Mock<ILogger<ProjectService>>();
        _files = new Mock<IFiles>();
        _projectFolderService = new Mock<IProjectFolderService>();
        _projectInfoFileService = new Mock<IProjectInfoFileService>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [Constants.ConfigProjectFolder] = @"C:\Projects"
            })
            .Build();

        _projectService = new ProjectService(
            _projectRepository.Object,
            _logger.Object,
            _files.Object,
            _projectFolderService.Object,
            _configuration,
            _projectInfoFileService.Object
        );
    }

    [Test]
    public void GetProjectCount_ReturnsCount()
    {
        // Mocks
        _projectRepository
            .Setup(r => r.GetProjectCount())
            .Returns(7);

        // Execution
        int result = _projectService.GetProjectCount();

        // Asserts
        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public void GetProjectById_ReturnsProject()
    {
        Project expectedProject = new Project(
            "Merijn",
            @"C:\2024-07-04-Merijn",
            new DateOnly(2024, 7, 4),
            2
        );

        // Mocks
        _projectRepository
            .Setup(r => r.GetById(2))
            .Returns(expectedProject);

        // Execution
        Project? result = _projectService.GetProjectById(2);

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProject));
    }

    [Test]
    public void GetAllProjects_ReturnsProjects()
    {
        List<Project> expectedProjects =
        [
            new Project("Ardennen", @"C:\2026-06-01-Ardennen", new DateOnly(2026, 6, 1), 0, 1),
            new Project("Antwerpen", @"C:\2024-01-10-Antwerpen", new DateOnly(2024, 1, 10), 0, 2)
        ];

        // Mocks
        _projectRepository
            .Setup(r => r.GetAll())
            .Returns(expectedProjects);

        // Execution
        List<Project> result = _projectService.GetAllProjects();

        // Asserts
        Assert.That(result, Is.EqualTo(expectedProjects));
    }

    [Test]
    public void CreateProject_CreatesProject()
    {
        const string projectRootFolder = @"C:\Projects";
        const string projectPath = @"C:\Projects\2024-07-04-Merijn";

        DateOnly date = new DateOnly(2024, 7, 4);

        // Mocks
        _files
            .Setup(f => f.Combine(projectRootFolder, "2024-07-04-Merijn"))
            .Returns(projectPath);

        _files
            .Setup(f => f.Exists(projectPath))
            .Returns(false);

        _projectRepository
            .Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                2
            ));

        // Execution
        Project result = _projectService.CreateProject("Merijn", date);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(2));
            Assert.That(result.Name, Is.EqualTo("Merijn"));
            Assert.That(result.Path, Is.EqualTo(projectPath));
            Assert.That(result.EventDate, Is.EqualTo(date));
            Assert.That(result.ParentProjectId, Is.Null);
        }

        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Merijn" &&
            p.Path == projectPath &&
            p.EventDate == date &&
            p.ParentProjectId == null
        )), Times.Once);

        _projectFolderService.Verify(s => s.CreateRequiredFolders(result), Times.Once);
        _projectInfoFileService.Verify(s => s.WriteProjectInfoFile(result), Times.Once);
    }

    [Test]
    public void CreateProject_FolderAlreadyExists_ThrowsException()
    {
        const string projectRootFolder = @"C:\Projects";
        const string projectPath = @"C:\Projects\2024-07-04-Merijn";

        DateOnly date = new DateOnly(2024, 7, 4);

        // Mocks
        _files
            .Setup(f => f.Combine(projectRootFolder, "2024-07-04-Merijn"))
            .Returns(projectPath);

        _files
            .Setup(f => f.Exists(projectPath))
            .Returns(true);

        // Execution & Assert
        Assert.Throws<InvalidOperationException>(() => _projectService.CreateProject("Merijn", date));

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.IsAny<Project>()), Times.Never);
        _projectFolderService.Verify(s => s.CreateRequiredFolders(It.IsAny<Project>()), Times.Never);
        _projectInfoFileService.Verify(s => s.WriteProjectInfoFile(It.IsAny<Project>()), Times.Never);
    }

    [Test]
    public void CreateProject_EmptyName_ThrowsException()
    {
        // Execution & Assert
        Assert.Throws<ArgumentException>(() => _projectService.CreateProject("", new DateOnly(2024, 7, 4)));

        // Asserts
        _projectRepository.Verify(r => r.Insert(It.IsAny<Project>()), Times.Never);
    }

    [Test]
    public void CreateSubProject_CreatesSubProject()
    {
        const int parentProjectId = 2;

        DateOnly date = new DateOnly(2024, 7, 4);

        Project parentProject = new Project(
            "Merijn",
            @"C:\Projects\2024-07-04-Merijn",
            date,
            null,
            parentProjectId
        );

        const string subProjectPath = @"C:\Projects\2024-07-04-Merijn\.Ceremony";

        // Mocks
        _projectRepository
            .Setup(r => r.GetById(parentProjectId))
            .Returns(parentProject);

        _files
            .Setup(f => f.Combine(parentProject.Path, ".Ceremony"))
            .Returns(subProjectPath);

        _files
            .Setup(f => f.Exists(subProjectPath))
            .Returns(false);

        _projectRepository
            .Setup(r => r.Insert(It.IsAny<Project>()))
            .Returns((Project project) => new Project(
                project.Name,
                project.Path,
                project.EventDate,
                project.ParentProjectId,
                3
            ));

        // Execution
        Project result = _projectService.CreateSubProject(parentProjectId, "Ceremony");

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(3));
            Assert.That(result.Name, Is.EqualTo("Ceremony"));
            Assert.That(result.Path, Is.EqualTo(subProjectPath));
            Assert.That(result.EventDate, Is.EqualTo(date));
            Assert.That(result.ParentProjectId, Is.EqualTo(parentProjectId));
        }

        _projectRepository.Verify(r => r.Insert(It.Is<Project>(p =>
            p.Id == null &&
            p.Name == "Ceremony" &&
            p.Path == subProjectPath &&
            p.EventDate == date &&
            p.ParentProjectId == parentProjectId
        )), Times.Once);

        _projectFolderService.Verify(s => s.CreateRequiredFolders(result), Times.Once);
        _projectInfoFileService.Verify(s => s.WriteProjectInfoFile(result), Times.Once);
    }
}