using Application.interfaces.infrastructure.repositories;
using Application.interfaces.services.project;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class ProjectServiceIntegrationTests : FileSystemIntegrationTestBase
{
    [Test]
    public void GetProjectCount_WithInsertedProjects_ReturnsProjectCount()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectService projectService =
            scope.ServiceProvider.GetRequiredService<IProjectService>();

        // Setup
        string firstProjectPath = CreateDirectory("2026-06-17-TestProject");
        string secondProjectPath = CreateDirectory("2026-03-17-TestProject2");

        Project firstProject = CreateProject(
            name: "Test Project",
            path: firstProjectPath,
            eventDate: new DateOnly(2026, 6, 17));

        Project secondProject = CreateProject(
            name: "Test Project2",
            path: secondProjectPath,
            eventDate: new DateOnly(2026, 3, 17));

        projectRepository.Insert(firstProject);
        projectRepository.Insert(secondProject);

        // Execution
        int projectCount = projectService.GetProjectCount();

        // Asserts
        Assert.That(projectCount, Is.EqualTo(2));
    }

    [Test]
    public void GetProjectById_WithInsertedProject_ReturnsProject()
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
        Project? retrievedProject = projectService.GetProjectById((int)insertedProject.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedProject, Is.Not.Null);
            Assert.That(retrievedProject!.Id, Is.EqualTo(insertedProject.Id));
            Assert.That(retrievedProject.Name, Is.EqualTo("Test Project"));
            Assert.That(retrievedProject.Path, Is.EqualTo(TemporaryDirectory));
            Assert.That(retrievedProject.EventDate, Is.EqualTo(new DateOnly(2026, 6, 17)));
        }
    }

    [Test]
    public void GetAllProjects_WithInsertedProjects_ReturnsProjects()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IProjectService projectService =
            scope.ServiceProvider.GetRequiredService<IProjectService>();

        // Setup
        string firstProjectPath = CreateDirectory("2026-06-17-TestProject");
        string secondProjectPath = CreateDirectory("2026-03-17-TestProject2");

        Project firstProject = CreateProject(
            name: "Test Project",
            path: firstProjectPath,
            eventDate: new DateOnly(2026, 6, 17));

        Project secondProject = CreateProject(
            name: "Test Project2",
            path: secondProjectPath,
            eventDate: new DateOnly(2026, 3, 17));

        Project insertedFirstProject = projectRepository.Insert(firstProject);
        Project insertedSecondProject = projectRepository.Insert(secondProject);

        // Execution
        List<Project> projects = projectService.GetAllProjects();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projects, Has.Count.EqualTo(2));
            Assert.That(projects.Select(project => project.Id), Does.Contain(insertedFirstProject.Id));
            Assert.That(projects.Select(project => project.Id), Does.Contain(insertedSecondProject.Id));
        }
    }
}