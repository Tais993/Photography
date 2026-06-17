using Application.interfaces;
using Application.services.interfaces;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Application.Constants;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class ProjectServiceIntegrationTests : FileSystemIntegrationTestBase
{


    [Test]
    public void ResolveProject_WithProjectInfoFile_ReturnsProject()
    {
        using IServiceScope scope = CreateScope();

        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        IProjectService projectService =
            scope.ServiceProvider.GetRequiredService<IProjectService>();

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
        Project? resolvedProject = projectService.ResolveProject(TemporaryDirectory);

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

        IProjectService projectService =
            scope.ServiceProvider.GetRequiredService<IProjectService>();

        // Setup
        Project project = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));

        Project insertedProject = projectRepository.Insert(project);

        // Execution
        Project? resolvedProject = projectService.ResolveProject(TemporaryDirectory, (int) insertedProject.Id!);

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
    public void ResolveProjectId_WithoutProjectInfoFile_ReturnsZero()
    {
            using IServiceScope scope = CreateScope();

            IProjectRepository projectRepository =
                scope.ServiceProvider.GetRequiredService<IProjectRepository>();

            IProjectService projectService =
                scope.ServiceProvider.GetRequiredService<IProjectService>();

            // Setup
            Project project = CreateProject(
                name: "Test Project",
                path: TemporaryDirectory,
                eventDate: new DateOnly(2026, 6, 17));

            projectRepository.Insert(project);

            // Execution
            Project? resolvedProject = projectService.ResolveProject(TemporaryDirectory);

            // Asserts
            using (Assert.EnterMultipleScope())
            {
                Assert.That(resolvedProject, Is.Null);
            }
    }
    
    [Test]
    public void GetProjectCount_WithInsertedProjects_ReturnsProjectCount () 
    {
        using IServiceScope scope = CreateScope();

        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        IProjectService projectService =
            scope.ServiceProvider.GetRequiredService<IProjectService>();

        // Setup
        Project firstProject = CreateProject(
            name: "Test Project",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 6, 17));
        
        Project secondProject = CreateProject(
            name: "Test Project2",
            path: TemporaryDirectory,
            eventDate: new DateOnly(2026, 3, 17));

        projectRepository.Insert(firstProject);
        projectRepository.Insert(secondProject);

        // Execution
        int projectCount = projectService.GetProjectCount();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projectCount, Is.EqualTo(2));
        }
    }
}