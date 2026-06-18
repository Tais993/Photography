using Application.interfaces.infrastructure;
using Application.interfaces.website;
using Domain.entities;
using Domain.website;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Fixtures;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class SelectionIndexServiceIntegrationTests : IntegrationTestBase
{
    
    [Test]
    public void GetSelectionIndex_WithoutSelectedProject_ReturnsEmptyViewModel()
    {
        using IServiceScope scope = CreateScope();
        ISelectionIndexService selectionIndexService =
            scope.ServiceProvider.GetRequiredService<ISelectionIndexService>();

        // Setup
        SelectionIndexRequest request = new()
        {
            PageNumber = 1,
            PageSize = 20
        };

        // Execution
        SelectionIndexViewModel viewModel =
            selectionIndexService.GetSelectionIndex(request);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.SelectedProjectId, Is.Null);
            Assert.That(viewModel.SelectedProject, Is.Null);
            Assert.That(viewModel.SelectedImage, Is.Null);
            Assert.That(viewModel.Images, Is.Empty);
            Assert.That(viewModel.SelectedImageIds, Is.Empty);
            Assert.That(viewModel.ProjectImageCount, Is.EqualTo(0));
            Assert.That(viewModel.PageNumber, Is.EqualTo(1));
            Assert.That(viewModel.PageSize, Is.EqualTo(20));
        }
    }

    [Test]
    public void GetSelectionIndex_WithSelectedProject_ReturnsProjectImagesAndStartsSession()
    {
        using IServiceScope scope = CreateScope();
        ISelectionIndexService selectionIndexService =
            scope.ServiceProvider.GetRequiredService<ISelectionIndexService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Setup
        Project project = projectRepository.Insert(CreateProject());

        imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0001",
            fileType: ".jpg",
            relationalFilePath: "Original/DSC_0001.jpg"));

        imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0001",
            fileType: ".NEF",
            relationalFilePath: "Original/DSC_0001.NEF"));

        imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0002",
            fileType: ".jpg",
            relationalFilePath: "Original/DSC_0002.jpg"));

        SelectionIndexRequest request = new()
        {
            SelectedProjectId = project.Id,
            PageNumber = 1,
            PageSize = 20
        };

        // Execution
        SelectionIndexViewModel viewModel =
            selectionIndexService.GetSelectionIndex(request);

        SelectionSession session =
            selectionRepository.GetByProject((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.SelectedProjectId, Is.EqualTo(project.Id));
            Assert.That(viewModel.SelectedProject, Is.Not.Null);
            Assert.That(viewModel.SelectedProject!.Id, Is.EqualTo(project.Id));

            Assert.That(viewModel.ProjectImageCount, Is.EqualTo(3));

            Assert.That(viewModel.Images, Has.Count.EqualTo(2));
            Assert.That(viewModel.Images.Any(image =>
                image.FileName == "DSC_0001" &&
                image.FileType == ".jpg"), Is.True);

            Assert.That(viewModel.Images.Any(image =>
                image.FileName == "DSC_0001" &&
                image.FileType == ".NEF"), Is.False);

            Assert.That(viewModel.Images.Any(image =>
                image.FileName == "DSC_0002" &&
                image.FileType == ".jpg"), Is.True);

            Assert.That(viewModel.SelectedImageIds, Is.Empty);
            Assert.That(session.ProjectId, Is.EqualTo(project.Id));
            Assert.That(session.Name, Is.EqualTo(project.Name));

            Assert.That(viewModel.ImagePage.PageNumber, Is.EqualTo(1));
            Assert.That(viewModel.ImagePage.PageSize, Is.EqualTo(20));
            Assert.That(viewModel.ImagePage.TotalItems, Is.EqualTo(2));
        }
    }

    [Test]
    public void GetSelectionIndex_WithSelectedImage_ReturnsSelectedImageViewModel()
    {
        using IServiceScope scope = CreateScope();
        ISelectionIndexService selectionIndexService =
            scope.ServiceProvider.GetRequiredService<ISelectionIndexService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();

        // Setup
        Project project = projectRepository.Insert(CreateProject());

        Image image = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0100",
            fileType: ".jpg",
            relationalFilePath: "Original/DSC_0100.jpg"));

        SelectionIndexRequest request = new()
        {
            SelectedProjectId = project.Id,
            SelectedImageId = image.Id,
            PageNumber = 1,
            PageSize = 20
        };

        // Execution
        SelectionIndexViewModel viewModel =
            selectionIndexService.GetSelectionIndex(request);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.SelectedImage, Is.Not.Null);
            Assert.That(viewModel.SelectedImage!.Image.Id, Is.EqualTo(image.Id));
            Assert.That(viewModel.SelectedImage.Image.FileName, Is.EqualTo("DSC_0100"));

            Assert.That(viewModel.SelectedImage.CanOpenInImageViewer, Is.False);
            Assert.That(viewModel.SelectedImage.ImageViewerName, Is.EqualTo("Unavailable"));
        }
    }

    [Test]
    public void ToggleImageSelection_TogglesImageSelection()
    {
        using IServiceScope scope = CreateScope();
        ISelectionIndexService selectionIndexService =
            scope.ServiceProvider.GetRequiredService<ISelectionIndexService>();
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        IImageRepository imageRepository =
            scope.ServiceProvider.GetRequiredService<IImageRepository>();
        ISelectionRepository selectionRepository =
            scope.ServiceProvider.GetRequiredService<ISelectionRepository>();

        // Setup
        Project project = projectRepository.Insert(CreateProject());

        Image image = imageRepository.Insert(CreateImage(
            projectId: (int)project.Id!,
            fileName: "DSC_0200",
            fileType: ".jpg",
            relationalFilePath: "Original/DSC_0200.jpg"));

        selectionRepository.StartSession((int)project.Id!, project.Name);

        // Execution
        ImageViewModel? selectedImage =
            selectionIndexService.ToggleImageSelection(
                (int)image.Id!,
                (int)project.Id!);

        ImageViewModel? deselectedImage =
            selectionIndexService.ToggleImageSelection(
                (int)image.Id!,
                (int)project.Id!);

        SelectionSession session =
            selectionRepository.GetByProject((int)project.Id!);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(selectedImage, Is.Not.Null);
            Assert.That(selectedImage!.ImageId, Is.EqualTo(image.Id));
            Assert.That(selectedImage.Selected, Is.True);

            Assert.That(deselectedImage, Is.Not.Null);
            Assert.That(deselectedImage!.ImageId, Is.EqualTo(image.Id));
            Assert.That(deselectedImage.Selected, Is.False);

            Assert.That(session.ImageIds, Is.Empty);
        }
    }

    [Test]
    public void ToggleImageSelection_WithMissingImage_ReturnsNull()
    {
        using IServiceScope scope = CreateScope();

        ISelectionIndexService selectionIndexService =
            scope.ServiceProvider.GetRequiredService<ISelectionIndexService>();

        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        // Setup
        Project project = projectRepository.Insert(CreateProject());

        // Execution
        ImageViewModel? result =
            selectionIndexService.ToggleImageSelection(
                imageId: 999,
                selectedProjectId: (int)project.Id!);

        // Asserts
        Assert.That(result, Is.Null);
    }
}