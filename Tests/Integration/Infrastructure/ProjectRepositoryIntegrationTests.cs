using Application.interfaces;
using Domain.entities;
using Domain.entities.search;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;

namespace Tests.Integration.Infrastructure;

public class ProjectRepositoryIntegrationTests : IntegrationTestBase
{
    [Test]
    public void Insert_ThenGetById_ReturnsProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        string projectName = "Tests Project";
        string projectPath = @"C:\Projects\Test Project";
        DateOnly projectEventDate = new DateOnly(2026, 6, 17);

        Project project = CreateProject(
            name: projectName,
            path: projectPath,
            eventDate: projectEventDate);

        // Execution
        Project insertedProject = projectRepository.Insert(project);
        Project retrievedProject = projectRepository.GetById((int)insertedProject.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(insertedProject, Is.Not.Null);
            Assert.That(retrievedProject, Is.Not.Null);

            Assert.That(retrievedProject.Id, Is.EqualTo(insertedProject.Id));
            Assert.That(retrievedProject.Name, Is.EqualTo(projectName));
            Assert.That(retrievedProject.Path, Is.EqualTo(projectPath));
            Assert.That(retrievedProject.EventDate, Is.EqualTo(projectEventDate));
            Assert.That(retrievedProject.ParentProjectId, Is.Null);
        }
    }

    [Test]
    public void InsertProjectWithParent_ThenGetById_ReturnsProjectWithParentId()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        Project parentProject = CreateProject(
            name: "Parent Test Project",
            path: @"C:\Projects\Parent\Test Project",
            eventDate: new DateOnly(2024, 6, 17));

        // Execution
        Project insertedParentProject = projectRepository.Insert(parentProject);

        Project project = CreateProject(
            name: "Tests Project",
            path: @"C:\Projects\Test Project",
            eventDate: new DateOnly(2026, 6, 17),
            parentProjectId: insertedParentProject.Id);

        // Execution
        Project insertedProject = projectRepository.Insert(project);
        Project retrievedProject = projectRepository.GetById((int)insertedProject.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(insertedProject, Is.Not.Null);
            Assert.That(retrievedProject, Is.Not.Null);
            Assert.That(retrievedProject.ParentProjectId, Is.EqualTo(insertedParentProject.Id));
        }
    }

    [Test]
    public void Update_ThenGetById_ReturnsUpdatedProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        string projectName = "Tests Project";
        string projectPath = @"C:\Projects\Test Project";
        DateOnly projectEventDate = new DateOnly(2026, 6, 17);

        Project project = CreateProject(
            name: projectName,
            path: projectPath,
            eventDate: projectEventDate);

        // Execution
        Project insertedProject = projectRepository.Insert(project);

        // Setup
        Project updatedProject = new Project(
            insertedProject.Id,
            "New project name",
            projectPath,
            projectEventDate,
            null);

        // Execution
        projectRepository.Update(updatedProject);
        Project retrievedProject = projectRepository.GetById((int)insertedProject.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(insertedProject, Is.Not.Null);
            Assert.That(retrievedProject, Is.Not.Null);

            Assert.That(retrievedProject.Id, Is.EqualTo(insertedProject.Id));

            Assert.That(insertedProject.Name, Is.EqualTo(projectName));
            Assert.That(retrievedProject.Name, Is.EqualTo(updatedProject.Name));

            Assert.That(retrievedProject.Path, Is.EqualTo(projectPath));
            Assert.That(retrievedProject.EventDate, Is.EqualTo(projectEventDate));

            Assert.That(retrievedProject.ParentProjectId, Is.Null);
        }
    }

    [Test]
    public void DeleteById_RemovesProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        Project project = CreateProject();

        Project insertedProject = projectRepository.Insert(project);
        int projectCountBeforeDeletion = projectRepository.GetProjectCount();

        // Execution
        projectRepository.DeleteById((int)insertedProject.Id!);

        Project? deletedProject = projectRepository.GetById((int)insertedProject.Id!);
        int projectCountAfterDeletion = projectRepository.GetProjectCount();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deletedProject, Is.Null);
            Assert.That(projectCountBeforeDeletion, Is.EqualTo(1));
            Assert.That(projectCountAfterDeletion, Is.EqualTo(0));
        }
    }

    [Test]
    public void SearchProjects_ByName_ReturnsMatchingProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        string projectName = "StillMarillion";
        string projectPath = @"C:\Projects\StillMarillion";
        DateOnly projectEventDate = new DateOnly(2026, 6, 17);

        Project matchingProject = CreateProject(
            name: projectName,
            path: projectPath,
            eventDate: projectEventDate);

        Project differentProject = CreateProject(
            name: "Narvik",
            path: @"C:\Projects\Narvik",
            eventDate: new DateOnly(2025, 8, 3));

        Project insertedMatchingProject = projectRepository.Insert(matchingProject);
        projectRepository.Insert(differentProject);

        ProjectSearchSettings searchSettings = new ProjectSearchSettings
        {
            ProjectName = "marillion"
        };

        // Execution
        List<Project> projects = projectRepository.SearchProjects(searchSettings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projects, Has.Count.EqualTo(1));

            Assert.That(projects[0].Id, Is.EqualTo(insertedMatchingProject.Id));
            Assert.That(projects[0].Name, Is.EqualTo(projectName));
            Assert.That(projects[0].Path, Is.EqualTo(projectPath));
            Assert.That(projects[0].EventDate, Is.EqualTo(projectEventDate));
            Assert.That(projects[0].ParentProjectId, Is.Null);
        }
    }

    [Test]
    public void InsertMultiple_ReturnsAll()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        Project firstProject = CreateProject(
            name: "StillMarillion",
            path: @"C:\Projects\StillMarillion",
            eventDate: new DateOnly(2026, 6, 17));

        Project secondProject = CreateProject(
            name: "Narvik",
            path: @"C:\Projects\Narvik",
            eventDate: new DateOnly(2025, 8, 3));

        // Execution
        projectRepository.Insert(firstProject);
        projectRepository.Insert(secondProject);

        List<Project> projects = projectRepository.GetAll();
        int projectCount = projectRepository.GetProjectCount();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projects, Has.Count.EqualTo(2));
            Assert.That(projectCount, Is.EqualTo(2));
        }
    }

    private static Project CreateProject(
        string name = "Test Project",
        string path = @"C:\Projects\Test Project",
        DateOnly? eventDate = null,
        int? parentProjectId = null)
    {
        return new Project(
            null,
            name,
            path,
            eventDate ?? new DateOnly(2026, 6, 17),
            parentProjectId);
    }
}