using Application.interfaces.infrastructure;
using Application.interfaces.website;
using Domain.entities;
using Domain.website;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class ProjectIndexServiceIntegrationTests : IntegrationTestBase
{
    [Test]
    public void GetProjectIndex_WithoutSelectedProject_SelectsFirstProjectFromPage()
    {
        using IServiceScope scope = CreateScope();
        IProjectIndexService projectIndexService =
            scope.ServiceProvider.GetRequiredService<IProjectIndexService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        Project olderProject = projectRepository.Insert(CreateProject(
            name: "Older Project",
            path: @"C:\Projects\Older",
            eventDate: new DateOnly(2025, 1, 1)));

        Project newestProject = projectRepository.Insert(CreateProject(
            name: "Newest Project",
            path: @"C:\Projects\Newest",
            eventDate: new DateOnly(2026, 6, 17)));

        Project middleProject = projectRepository.Insert(CreateProject(
            name: "Middle Project",
            path: @"C:\Projects\Middle",
            eventDate: new DateOnly(2026, 1, 1)));

        ProjectIndexRequest request = new()
        {
            ProjectPageNumber = 1,
            ProjectPageSize = 2
        };

        // Execution
        ProjectIndexViewModel viewModel =
            projectIndexService.GetProjectIndex(request);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.ProjectCount, Is.EqualTo(3));

            Assert.That(viewModel.ProjectPage.TotalItems, Is.EqualTo(3));
            Assert.That(viewModel.ProjectPage.TotalPages, Is.EqualTo(2));
            Assert.That(viewModel.ProjectPage.PageNumber, Is.EqualTo(1));
            Assert.That(viewModel.ProjectPage.PageSize, Is.EqualTo(2));

            Assert.That(viewModel.Projects, Has.Count.EqualTo(2));
            Assert.That(viewModel.Projects[0].Id, Is.EqualTo(newestProject.Id));
            Assert.That(viewModel.Projects[1].Id, Is.EqualTo(middleProject.Id));

            Assert.That(viewModel.SelectedProject, Is.Not.Null);
            Assert.That(viewModel.SelectedProject!.Id, Is.EqualTo(newestProject.Id));
            Assert.That(viewModel.SelectedProjectId, Is.EqualTo(newestProject.Id));

            Assert.That(olderProject.Id, Is.Not.Null);
        }
    }

    [Test]
    public void GetProjectIndex_WithSelectedProject_UsesSelectedProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectIndexService projectIndexService =
            scope.ServiceProvider.GetRequiredService<IProjectIndexService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        Project firstProject = projectRepository.Insert(CreateProject(
            name: "First Project",
            path: @"C:\Projects\First",
            eventDate: new DateOnly(2026, 6, 17)));

        Project selectedProject = projectRepository.Insert(CreateProject(
            name: "Selected Project",
            path: @"C:\Projects\Selected",
            eventDate: new DateOnly(2025, 1, 1)));

        ProjectIndexRequest request = new()
        {
            SelectedProjectId = selectedProject.Id,
            ProjectPageNumber = 1,
            ProjectPageSize = 20
        };

        // Execution
        ProjectIndexViewModel viewModel =
            projectIndexService.GetProjectIndex(request);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.ProjectCount, Is.EqualTo(2));

            Assert.That(viewModel.Projects, Has.Count.EqualTo(2));
            Assert.That(viewModel.Projects[0].Id, Is.EqualTo(firstProject.Id));

            Assert.That(viewModel.SelectedProjectId, Is.EqualTo(selectedProject.Id));
            Assert.That(viewModel.SelectedProject, Is.Not.Null);
            Assert.That(viewModel.SelectedProject!.Id, Is.EqualTo(selectedProject.Id));
            Assert.That(viewModel.SelectedProject.Name, Is.EqualTo("Selected Project"));
        }
    }

    [Test]
    public void GetProjectIndex_WithSearchFilter_ReturnsMatchingProjects()
    {
        using IServiceScope scope = CreateScope();
        IProjectIndexService projectIndexService =
            scope.ServiceProvider.GetRequiredService<IProjectIndexService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        Project concertProject = projectRepository.Insert(CreateProject(
            name: "Concert Project",
            path: @"C:\Projects\Concert",
            eventDate: new DateOnly(2026, 6, 17)));

        projectRepository.Insert(CreateProject(
            name: "Wedding Project",
            path: @"C:\Projects\Wedding",
            eventDate: new DateOnly(2026, 6, 18)));

        ProjectIndexRequest request = new()
        {
            Search = "Concert",
            ProjectPageNumber = 1,
            ProjectPageSize = 20
        };

        // Execution
        ProjectIndexViewModel viewModel =
            projectIndexService.GetProjectIndex(request);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.ProjectCount, Is.EqualTo(2));

            Assert.That(viewModel.ProjectPage.TotalItems, Is.EqualTo(1));
            Assert.That(viewModel.Projects, Has.Count.EqualTo(1));
            Assert.That(viewModel.Projects[0].Id, Is.EqualTo(concertProject.Id));

            Assert.That(viewModel.SelectedProject, Is.Not.Null);
            Assert.That(viewModel.SelectedProject!.Id, Is.EqualTo(concertProject.Id));
            Assert.That(viewModel.SelectedProjectId, Is.EqualTo(concertProject.Id));
        }
    }

    [Test]
    public void GetProjectIndex_WithProjectPathFilter_ReturnsMatchingProjects()
    {
        using IServiceScope scope = CreateScope();
        IProjectIndexService projectIndexService =
            scope.ServiceProvider.GetRequiredService<IProjectIndexService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        Project archiveProject = projectRepository.Insert(CreateProject(
            name: "Archive Project",
            path: @"D:\Archive\Photography\Archive Project",
            eventDate: new DateOnly(2026, 6, 17)));

        projectRepository.Insert(CreateProject(
            name: "Local Project",
            path: @"C:\Projects\Local Project",
            eventDate: new DateOnly(2026, 6, 18)));

        ProjectIndexRequest request = new()
        {
            ProjectPath = "Archive",
            ProjectPageNumber = 1,
            ProjectPageSize = 20
        };

        // Execution
        ProjectIndexViewModel viewModel =
            projectIndexService.GetProjectIndex(request);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.ProjectPage.TotalItems, Is.EqualTo(1));
            Assert.That(viewModel.Projects, Has.Count.EqualTo(1));
            Assert.That(viewModel.Projects[0].Id, Is.EqualTo(archiveProject.Id));
            Assert.That(viewModel.SelectedProjectId, Is.EqualTo(archiveProject.Id));
        }
    }

    [Test]
    public void CreateProjectView_MapsProjectAndSelectedState()
    {
        using IServiceScope scope = CreateScope();
        IProjectIndexService projectIndexService =
            scope.ServiceProvider.GetRequiredService<IProjectIndexService>();

        // Setup
        Project project = CreateProject(
            id: 5,
            name: "Mapped Project",
            path: @"C:\Projects\Mapped",
            eventDate: new DateOnly(2026, 6, 17));

        // Execution
        ProjectViewModel selectedView =
            projectIndexService.CreateProjectView(project, selectedProjectId: 5);

        ProjectViewModel unselectedView =
            projectIndexService.CreateProjectView(project, selectedProjectId: 10);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(selectedView.Selected, Is.True);
            Assert.That(selectedView.Id, Is.EqualTo(5));
            Assert.That(selectedView.Name, Is.EqualTo("Mapped Project"));
            Assert.That(selectedView.Path, Is.EqualTo(@"C:\Projects\Mapped"));
            Assert.That(selectedView.EventDate, Is.EqualTo(new DateOnly(2026, 6, 17)));

            Assert.That(unselectedView.Selected, Is.False);
        }
    }
}