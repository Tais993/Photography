using Application.interfaces.infrastructure;
using Application.interfaces.services.project;
using Application.services.project;
using Domain.entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit.Application.services;

[TestFixture]
[TestOf(typeof(ProjectService))]
public class ProjectServiceTest
{
    private Mock<IProjectRepository> _projectRepository = null!;
    private Mock<ILogger<ProjectService>> _logger = null!;
    private Mock<IFiles> _files = null!;
    private Mock<IProjectFolderService> _projectFolderService = null!;
    private IConfiguration _configuration = null!;

    private ProjectService _projectService = null!;

    [SetUp]
    public void SetUp()
    {
        _projectRepository = new Mock<IProjectRepository>();
        _logger = new Mock<ILogger<ProjectService>>();
        _files = new Mock<IFiles>();
        _projectFolderService = new Mock<IProjectFolderService>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Add config values here if your ProjectService needs them
            })
            .Build();

        _projectService = new ProjectService(
            _projectRepository.Object,
            _logger.Object,
            _files.Object,
            _projectFolderService.Object,
            _configuration
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
}