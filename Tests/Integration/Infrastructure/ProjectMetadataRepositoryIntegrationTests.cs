using Application.interfaces.infrastructure;
using Domain.entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Infrastructure;

public class ProjectMetadataRepositoryIntegrationTests : IntegrationTestBase
{
    [Test]
    public void Insert_ThenGetByKey_ReturnsProjectMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IProjectMetadataRepository projectMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IProjectMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Metadata setup
        const string metadataKey = "event_type";
        const string metadataType = "string";
        const string displayName = "Event Type";
        const string description = "The type of photography event.";

        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            metadataType: metadataType,
            displayName: displayName,
            description: description));

        // Setup
        const string metadataValue = "Concert";

        ProjectMetadata projectMetadata = CreateProjectMetadata(
            projectId: (int)project.Id!,
            metadata: metadata,
            metadataValue: metadataValue);

        // Execution
        projectMetadataRepository.Insert(projectMetadata);
        ProjectMetadata? retrievedProjectMetadata =
            projectMetadataRepository.GetByKey((int)project.Id!, metadataKey);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedProjectMetadata, Is.Not.Null);

            Assert.That(retrievedProjectMetadata!.ProjectId, Is.EqualTo(project.Id));
            Assert.That(retrievedProjectMetadata.MetadataKey, Is.EqualTo(metadataKey));
            Assert.That(retrievedProjectMetadata.MetadataValue, Is.EqualTo(metadataValue));
            Assert.That(retrievedProjectMetadata.MetadataType, Is.EqualTo(metadataType));
            Assert.That(retrievedProjectMetadata.DisplayName, Is.EqualTo(displayName));
            Assert.That(retrievedProjectMetadata.Description, Is.EqualTo(description));
        }
    }

    [Test]
    public void InsertMultiple_GetAllByProjectId_ReturnsOnlyProjectMetadataForProject()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IProjectMetadataRepository projectMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IProjectMetadataRepository>();

        // Project setup
        Project firstProject = projectRepository.Insert(CreateProject(name: "First Project"));
        Project secondProject = projectRepository.Insert(CreateProject(name: "Second Project"));

        // Metadata setup
        Metadata eventTypeMetadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: "event_type",
            displayName: "Event Type"));

        Metadata locationMetadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: "location",
            displayName: "Location"));

        // Setup
        ProjectMetadata firstProjectEventType = CreateProjectMetadata(
            projectId: (int)firstProject.Id!,
            metadata: eventTypeMetadata,
            metadataValue: "Concert");

        ProjectMetadata firstProjectLocation = CreateProjectMetadata(
            projectId: (int)firstProject.Id!,
            metadata: locationMetadata,
            metadataValue: "Uden");

        ProjectMetadata secondProjectEventType = CreateProjectMetadata(
            projectId: (int)secondProject.Id!,
            metadata: eventTypeMetadata,
            metadataValue: "Travel");

        // Execution
        projectMetadataRepository.Insert(firstProjectEventType);
        projectMetadataRepository.Insert(firstProjectLocation);
        projectMetadataRepository.Insert(secondProjectEventType);

        List<ProjectMetadata> projectMetadata =
            projectMetadataRepository.GetAllByProjectId((int)firstProject.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projectMetadata, Has.Count.EqualTo(2));

            Assert.That(projectMetadata.All(metadata => metadata.ProjectId == firstProject.Id), Is.True);
            Assert.That(projectMetadata.Select(metadata => metadata.MetadataKey), Does.Contain("event_type"));
            Assert.That(projectMetadata.Select(metadata => metadata.MetadataKey), Does.Contain("location"));
        }
    }

    [Test]
    public void Update_ThenGetByKey_ReturnsUpdatedProjectMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IProjectMetadataRepository projectMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IProjectMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Metadata setup
        const string metadataKey = "event_type";
        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            displayName: "Event Type"));

        // Setup
        const string originalMetadataValue = "Concert";
        const string updatedMetadataValue = "Festival";

        ProjectMetadata projectMetadata = CreateProjectMetadata(
            projectId: (int)project.Id!,
            metadata: metadata,
            metadataValue: originalMetadataValue);

        // Execution
        projectMetadataRepository.Insert(projectMetadata);

        ProjectMetadata updatedProjectMetadata = CreateProjectMetadata(
            projectId: (int)project.Id!,
            metadata: metadata,
            metadataValue: updatedMetadataValue);

        projectMetadataRepository.Update(updatedProjectMetadata);

        ProjectMetadata? retrievedProjectMetadata =
            projectMetadataRepository.GetByKey((int)project.Id!, metadataKey);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedProjectMetadata, Is.Not.Null);

            Assert.That(retrievedProjectMetadata!.ProjectId, Is.EqualTo(project.Id));
            Assert.That(retrievedProjectMetadata.MetadataKey, Is.EqualTo(metadataKey));
            Assert.That(retrievedProjectMetadata.MetadataValue, Is.EqualTo(updatedMetadataValue));
        }
    }

    [Test]
    public void DeleteByKey_RemovesProjectMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IProjectMetadataRepository projectMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IProjectMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Metadata setup
        const string metadataKey = "event_type";
        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            displayName: "Event Type"));

        // Setup
        ProjectMetadata projectMetadata = CreateProjectMetadata(
            projectId: (int)project.Id!,
            metadata: metadata,
            metadataValue: "Concert");

        // Execution
        projectMetadataRepository.Insert(projectMetadata);
        projectMetadataRepository.DeleteByKey((int)project.Id!, metadataKey);
        
        ProjectMetadata? retrievedProjectMetadata = projectMetadataRepository.GetByKey((int)project.Id!, metadataKey);
        List<ProjectMetadata> projectMetadataList =
            projectMetadataRepository.GetAllByProjectId((int)project.Id!);
        
        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedProjectMetadata, Is.Null);
            Assert.That(projectMetadataList, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void DeleteProject_CascadesProjectMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IProjectMetadataRepository projectMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IProjectMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Metadata setup
        const string metadataKey = "event_type";
        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            displayName: "Event Type"));

        // Setup
        ProjectMetadata projectMetadata = CreateProjectMetadata(
            projectId: (int)project.Id!,
            metadata: metadata,
            metadataValue: "Concert");

        // Execution
        projectMetadataRepository.Insert(projectMetadata);
        projectRepository.DeleteById((int)project.Id!);
            
        ProjectMetadata? retrievedMetadata = projectMetadataRepository.GetByKey((int) project.Id, metadata.MetadataKey!);
        List<ProjectMetadata> metadatas = projectMetadataRepository.GetAllByProjectId((int) project.Id);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedMetadata, Is.Null);
            Assert.That(metadatas, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void DeleteMetadata_CascadesProjectMetadata()
    {
        using IServiceScope scope = CreateScope();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IMetadataRepository metadataRepository =
            scope.ServiceProvider.GetRequiredService<IMetadataRepository>();
        IProjectMetadataRepository projectMetadataRepository =
            scope.ServiceProvider.GetRequiredService<IProjectMetadataRepository>();

        // Project setup
        Project project = projectRepository.Insert(CreateProject());

        // Metadata setup
        const string metadataKey = "event_type";
        Metadata metadata = metadataRepository.Insert(CreateMetadata(
            metadataKey: metadataKey,
            displayName: "Event Type"));

        // Setup
        ProjectMetadata projectMetadata = CreateProjectMetadata(
            projectId: (int)project.Id!,
            metadata: metadata,
            metadataValue: "Concert");

        // Execution
        projectMetadataRepository.Insert(projectMetadata);
        metadataRepository.DeleteByKey(metadataKey);

        Metadata? retrievedMetadata = metadataRepository.GetByKey(metadata.MetadataKey!);
        List<Metadata> metadatas = metadataRepository.GetAll();

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(retrievedMetadata, Is.Null);
            Assert.That(metadatas, Has.Count.EqualTo(0));
        }
    }
}