using Application.interfaces.infrastructure;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Infrastructure;

public class SelectionRepositoryIntegrationTests : IntegrationTestBase
{
    [Test]
    public void StartSession_ThenGetByProject_ReturnsSession()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        const string sessionName = "Test Selection";

        // Execution
        SelectionSession createdSession =
            selectionRepository.StartSession((int)project.Id!, sessionName);

        SelectionSession retrievedSession =
            selectionRepository.GetByProject((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdSession, Is.Not.Null);
            Assert.That(retrievedSession, Is.Not.Null);

            Assert.That(retrievedSession.Id, Is.EqualTo(createdSession.Id));
            Assert.That(retrievedSession.ProjectId, Is.EqualTo(project.Id));
            Assert.That(retrievedSession.Name, Is.EqualTo(sessionName));
            Assert.That(retrievedSession.ImageIds, Is.Empty);
        }
    }

    [Test]
    public void GetOrStartSession_WhenSessionDoesNotExist_CreatesSession()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        const string sessionName = "Test Selection";

        // Execution
        SelectionSession session =
            selectionRepository.GetOrStartSession((int)project.Id!, sessionName);

        SelectionSession retrievedSession =
            selectionRepository.GetByProject((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(session, Is.Not.Null);
            Assert.That(retrievedSession, Is.Not.Null);

            Assert.That(session.Id, Is.Not.Null);
            Assert.That(session.ProjectId, Is.EqualTo(project.Id));
            Assert.That(session.Name, Is.EqualTo(sessionName));

            Assert.That(retrievedSession.Id, Is.EqualTo(session.Id));
            Assert.That(retrievedSession.ProjectId, Is.EqualTo(project.Id));
            Assert.That(retrievedSession.Name, Is.EqualTo(sessionName));
        }
    }

    [Test]
    public void GetOrStartSession_WhenSessionExists_ReturnsExistingSession()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        const string originalSessionName = "Original Selection";
        const string newSessionName = "New Selection";

        SelectionSession originalSession =
            selectionRepository.StartSession((int)project.Id!, originalSessionName);

        // Execution
        SelectionSession returnedSession =
            selectionRepository.GetOrStartSession((int)project.Id!, newSessionName);

        SelectionSession retrievedSession =
            selectionRepository.GetByProject((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(originalSession, Is.Not.Null);
            Assert.That(returnedSession, Is.Not.Null);
            Assert.That(retrievedSession, Is.Not.Null);

            Assert.That(returnedSession.Id, Is.EqualTo(originalSession.Id));
            Assert.That(returnedSession.ProjectId, Is.EqualTo(project.Id));
            Assert.That(returnedSession.Name, Is.EqualTo(newSessionName));

            Assert.That(retrievedSession.Id, Is.EqualTo(originalSession.Id));
            Assert.That(retrievedSession.Name, Is.EqualTo(newSessionName));
        }
    }

    [Test]
    public void AddImageToProjectSelection_ThenImageIsSelected_ReturnsTrue()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Image setup
        Image image = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_1000"));

        // Selection setup
        SelectionSession session =
            selectionRepository.StartSession((int)project.Id!, "Test Selection");

        // Execution
        selectionRepository.AddImageToProjectSelection(
            (int)session.Id!,
            (int)image.Id!);

        bool imageIsSelected =
            selectionRepository.ImageIsSelected((int)session.Id!, (int)image.Id!);

        // Asserts
        Assert.That(imageIsSelected, Is.True);
    }

    [Test]
    public void RemoveImageFromProjectSelection_ThenImageIsSelected_ReturnsFalse()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Image setup
        Image image = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_1000"));

        // Selection setup
        SelectionSession session =
            selectionRepository.StartSession((int)project.Id!, "Test Selection");

        selectionRepository.AddImageToProjectSelection(
            (int)session.Id!,
            (int)image.Id!);

        bool imageIsSelectedBeforeRemoval =
            selectionRepository.ImageIsSelected((int)session.Id!, (int)image.Id!);

        // Execution
        selectionRepository.RemoveImageFromProjectSelection(
            (int)session.Id!,
            (int)image.Id!);

        bool imageIsSelectedAfterRemoval =
            selectionRepository.ImageIsSelected((int)session.Id!, (int)image.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageIsSelectedBeforeRemoval, Is.True);
            Assert.That(imageIsSelectedAfterRemoval, Is.False);
        }
    }

    [Test]
    public void ClearSession_RemovesSelectedImages()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Image setup
        Image image = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_1000"));

        // Selection setup
        SelectionSession session =
            selectionRepository.StartSession((int)project.Id!, "Test Selection");

        selectionRepository.AddImageToProjectSelection(
            (int)session.Id!,
            (int)image.Id!);

        bool imageIsSelectedBeforeClear =
            selectionRepository.ImageIsSelected((int)session.Id!, (int)image.Id!);

        // Execution
        selectionRepository.ClearSession((int)project.Id!);

        bool imageIsSelectedAfterClear =
            selectionRepository.ImageIsSelected((int)session.Id!, (int)image.Id!);

        SelectionSession retrievedSession =
            selectionRepository.GetByProject((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageIsSelectedBeforeClear, Is.True);
            Assert.That(imageIsSelectedAfterClear, Is.False);

            Assert.That(retrievedSession.Id, Is.EqualTo(session.Id));
            Assert.That(retrievedSession.ProjectId, Is.EqualTo(project.Id));
            Assert.That(retrievedSession.ImageIds, Is.Empty);
        }
    }

    [Test]
    public void RemoveSession_RemovesSession()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Setup
        SelectionSession session =
            selectionRepository.StartSession((int)project.Id!, "Test Selection");

        int? sessionIdBeforeRemoval =
            selectionRepository.GetSessionIdByProjectId((int)project.Id!);

        // Execution
        selectionRepository.RemoveSession((int)project.Id!);
        SelectionSession? retrievedSession = selectionRepository.GetByProject((int)project.Id!);
        int? retrievedSessionId = selectionRepository.GetSessionIdByProjectId((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(sessionIdBeforeRemoval, Is.EqualTo(session.Id));
            Assert.That(retrievedSession, Is.Null);
            Assert.That(retrievedSessionId, Is.EqualTo(0));
        }
    }

    [Test]
    public void GetByProject_ReturnsSessionWithSelectedImages()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Image setup
        Image firstImage = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_1000"));

        Image secondImage = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_1001"));

        Image unselectedImage = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_1002"));

        // Selection setup
        SelectionSession session =
            selectionRepository.StartSession((int)project.Id!, "Test Selection");

        selectionRepository.AddImageToProjectSelection(
            (int)session.Id!,
            (int)firstImage.Id!);

        selectionRepository.AddImageToProjectSelection(
            (int)session.Id!,
            (int)secondImage.Id!);

        // Execution
        SelectionSession retrievedSession =
            selectionRepository.GetByProject((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedSession, Is.Not.Null);

            Assert.That(retrievedSession.Id, Is.EqualTo(session.Id));
            Assert.That(retrievedSession.ProjectId, Is.EqualTo(project.Id));
            Assert.That(retrievedSession.Name, Is.EqualTo("Test Selection"));

            Assert.That(retrievedSession.ImageIds, Has.Count.EqualTo(2));
            Assert.That(retrievedSession.ImageIds, Does.Contain(firstImage.Id));
            Assert.That(retrievedSession.ImageIds, Does.Contain(secondImage.Id));
            Assert.That(retrievedSession.ImageIds, Does.Not.Contain(unselectedImage.Id));
        }
    }
}