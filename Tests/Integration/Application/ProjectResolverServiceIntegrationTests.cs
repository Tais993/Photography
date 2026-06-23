using Application.interfaces.infrastructure;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Application.Constants;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class ProjectResolverServiceIntegrationTests : FileSystemIntegrationTestBase
{
    [Test]
    public void ResolveProject_WithProjectInfoFile_ReturnsProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        Project insertedProject = projectRepository.Insert(project);

        File.WriteAllText(
            Path.Combine(TemporaryDirectory, ProjectInfoFile),
            insertedProject.Id.ToString());

        // Execution
        Project? resolvedProject = projectResolverService.ResolveProject(TemporaryDirectory);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(resolvedProject, Is.Not.Null);
            Assert.That(resolvedProject!.Id, Is.EqualTo(insertedProject.Id));
            Assert.That(resolvedProject.Name, Is.EqualTo("Test Project"));
            Assert.That(resolvedProject.Path, Is.EqualTo(TemporaryDirectory));
            Assert.That(resolvedProject.EventDate, Is.EqualTo(new DateOnly(2026, 6, 17)));
        }
    }

    [Test]
    public void ResolveProject_WithProjectInfoFileInParentFolder_ReturnsProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        string originalsPath = Path.Combine(TemporaryDirectory, "Originals");
        Directory.CreateDirectory(originalsPath);

        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        Project insertedProject = projectRepository.Insert(project);

        File.WriteAllText(
            Path.Combine(TemporaryDirectory, ProjectInfoFile),
            insertedProject.Id.ToString());

        // Execution
        Project? resolvedProject = projectResolverService.ResolveProject(originalsPath);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(resolvedProject, Is.Not.Null);
            Assert.That(resolvedProject!.Id, Is.EqualTo(insertedProject.Id));
            Assert.That(resolvedProject.Name, Is.EqualTo("Test Project"));
            Assert.That(resolvedProject.Path, Is.EqualTo(TemporaryDirectory));
            Assert.That(resolvedProject.EventDate, Is.EqualTo(new DateOnly(2026, 6, 17)));
        }
    }

    [Test]
    public void ResolveProject_WithProvidedProjectId_ReturnsProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        Project insertedProject = projectRepository.Insert(project);

        // Execution
        Project? resolvedProject = projectResolverService.ResolveProject(
            TemporaryDirectory,
            (int)insertedProject.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(resolvedProject, Is.Not.Null);
            Assert.That(resolvedProject!.Id, Is.EqualTo(insertedProject.Id));
            Assert.That(resolvedProject.Name, Is.EqualTo("Test Project"));
            Assert.That(resolvedProject.Path, Is.EqualTo(TemporaryDirectory));
            Assert.That(resolvedProject.EventDate, Is.EqualTo(new DateOnly(2026, 6, 17)));
        }
    }

    [Test]
    public void ResolveProject_WithoutProjectInfoFile_ReturnsNull()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        projectRepository.Insert(project);

        // Execution
        Project? resolvedProject = projectResolverService.ResolveProject(TemporaryDirectory);

        // Asserts
        Assert.That(resolvedProject, Is.Null);
    }

    [Test]
    public void ResolveProjectId_WithProjectInfoFile_ReturnsProjectId()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        Project insertedProject = projectRepository.Insert(project);

        File.WriteAllText(
            Path.Combine(TemporaryDirectory, ProjectInfoFile),
            insertedProject.Id.ToString());

        // Execution
        int resolvedProjectId = projectResolverService.ResolveProjectId(TemporaryDirectory);

        // Asserts
        Assert.That(resolvedProjectId, Is.EqualTo(insertedProject.Id));
    }

    [Test]
    public void ResolveProjectId_WithProjectInfoFileInParentFolder_ReturnsProjectId()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        string originalsPath = Path.Combine(TemporaryDirectory, "Originals");
        Directory.CreateDirectory(originalsPath);

        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        Project insertedProject = projectRepository.Insert(project);

        File.WriteAllText(
            Path.Combine(TemporaryDirectory, ProjectInfoFile),
            insertedProject.Id.ToString());

        // Execution
        int resolvedProjectId = projectResolverService.ResolveProjectId(originalsPath);

        // Asserts
        Assert.That(resolvedProjectId, Is.EqualTo(insertedProject.Id));
    }

    [Test]
    public void ResolveProjectId_WithProvidedProjectId_ReturnsProjectId()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        Project insertedProject = projectRepository.Insert(project);

        // Execution
        int resolvedProjectId = projectResolverService.ResolveProjectId(
            TemporaryDirectory,
            (int)insertedProject.Id!);

        // Asserts
        Assert.That(resolvedProjectId, Is.EqualTo(insertedProject.Id));
    }

    [Test]
    public void ResolveProjectId_WithoutProjectInfoFile_ReturnsZero()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        projectRepository.Insert(project);

        // Execution
        int resolvedProjectId = projectResolverService.ResolveProjectId(TemporaryDirectory);

        // Asserts
        Assert.That(resolvedProjectId, Is.EqualTo(0));
    }

    [Test]
    public void ResolveProjectId_WithInvalidProjectInfoFile_ReturnsZero()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectResolverService projectResolverService =
            scope.ServiceProvider.GetRequiredService<IProjectResolverService>();

        // Setup
        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        projectRepository.Insert(project);

        File.WriteAllText(
            Path.Combine(TemporaryDirectory, ProjectInfoFile),
            "invalid");

        // Execution
        int resolvedProjectId = projectResolverService.ResolveProjectId(TemporaryDirectory);

        // Asserts
        Assert.That(resolvedProjectId, Is.EqualTo(0));
    }
}