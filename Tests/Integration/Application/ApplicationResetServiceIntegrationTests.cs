using Application.interfaces.infrastructure;
using Application.interfaces.services;
using Domain.entities;
using Infrastructure.database;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Tests.Integration.Fixtures;
using static Application.Constants;
using static Tests.Integration.Utilities.IntegrationTestEntityFactory;

namespace Tests.Integration.Application;

public class ApplicationResetServiceIntegrationTests : StandardProjectIntegrationTestBase
{
    private static readonly string[] ApplicationTables =
    [
        "selection_session_image",
        "selection_session",
        "project_metadata",
        "metadata",
        "image",
        "project",
        "changelog"
    ];

    [Test]
    public void ResetApplicationData_WithDryRun_DoesNotDeleteProjectInfoFilesOrDropDatabase()
    {
        using IServiceScope scope = CreateScope();
        IApplicationResetService applicationResetService =
            scope.ServiceProvider.GetRequiredService<IApplicationResetService>();
        NpgsqlDataSource dataSource =
            scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

        // Setup
        string projectPath = CreateStandardProjectDirectory();
        Project project = InsertProject(scope, projectPath, "Dry run project");
        string projectInfoPath = CreateProjectInfoFileInStandardProject((int)project.Id!);

        // Execution
        applicationResetService.ResetApplicationData(true);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(File.Exists(projectInfoPath), Is.True);
            AssertApplicationTablesExist(dataSource);
        }
    }

    [Test]
    public void ResetApplicationData_WithoutDryRun_DeletesProjectInfoFilesAndDropsDatabase()
    {
        using IServiceScope scope = CreateScope();
        IApplicationResetService applicationResetService =
            scope.ServiceProvider.GetRequiredService<IApplicationResetService>();
        MigrationService migrationService =
            scope.ServiceProvider.GetRequiredService<MigrationService>();
        NpgsqlDataSource dataSource =
            scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

        // Setup
        string projectPath = CreateStandardProjectDirectory();
        Project project = InsertProject(scope, projectPath, "Reset project");
        string projectInfoPath = CreateProjectInfoFileInStandardProject((int)project.Id!);

        try
        {
            // Execution
            applicationResetService.ResetApplicationData();

            // Asserts
            using (Assert.EnterMultipleScope())
            {
                Assert.That(File.Exists(projectInfoPath), Is.False);
                AssertApplicationTablesDoNotExist(dataSource);
            }
        }
        finally
        {
            migrationService.Migrate();
        }
    }

    [Test]
    public void ResetApplicationData_WithoutDryRun_WhenProjectInfoFileDoesNotExist_DoesNotThrowAndDropsDatabase()
    {
        using IServiceScope scope = CreateScope();
        IApplicationResetService applicationResetService =
            scope.ServiceProvider.GetRequiredService<IApplicationResetService>();
        MigrationService migrationService =
            scope.ServiceProvider.GetRequiredService<MigrationService>();
        NpgsqlDataSource dataSource =
            scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

        // Setup
        string projectPath = CreateStandardProjectDirectory();
        string projectInfoPath = GetProjectInfoPath(DefaultProjectFolderName);

        InsertProject(scope, projectPath, "Missing project info project");

        try
        {
            // Execution & Asserts
            Assert.DoesNotThrow(() => applicationResetService.ResetApplicationData());

            // Asserts
            using (Assert.EnterMultipleScope())
            {
                Assert.That(File.Exists(projectInfoPath), Is.False);
                AssertApplicationTablesDoNotExist(dataSource);
            }
        }
        finally
        {
            migrationService.Migrate();
        }
    }

    [Test]
    public void ResetApplicationData_WithMultipleProjects_DeletesExistingProjectInfoFilesAndDropsDatabase()
    {
        using IServiceScope scope = CreateScope();
        IApplicationResetService applicationResetService =
            scope.ServiceProvider.GetRequiredService<IApplicationResetService>();
        MigrationService migrationService =
            scope.ServiceProvider.GetRequiredService<MigrationService>();
        NpgsqlDataSource dataSource =
            scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

        // Setup
        string firstProjectFolderName = "2026-06-22-FirstProject";
        string secondProjectFolderName = "2026-06-22-SecondProject";
        string thirdProjectFolderName = "2026-06-22-ThirdProject";

        string firstProjectPath = CreateDirectory(firstProjectFolderName);
        string secondProjectPath = CreateDirectory(secondProjectFolderName);
        string thirdProjectPath = CreateDirectory(thirdProjectFolderName);

        Project firstProject = InsertProject(scope, firstProjectPath, "First project");
        Project secondProject = InsertProject(scope, secondProjectPath, "Second project");
        InsertProject(scope, thirdProjectPath, "Third project");

        string firstProjectInfoPath = CreateProjectInfoFile(firstProjectFolderName, (int)firstProject.Id!);
        string secondProjectInfoPath = CreateProjectInfoFile(secondProjectFolderName, (int)secondProject.Id!);
        string thirdProjectInfoPath = GetProjectInfoPath(thirdProjectFolderName);

        try
        {
            // Execution
            applicationResetService.ResetApplicationData();

            // Asserts
            using (Assert.EnterMultipleScope())
            {
                Assert.That(File.Exists(firstProjectInfoPath), Is.False);
                Assert.That(File.Exists(secondProjectInfoPath), Is.False);
                Assert.That(File.Exists(thirdProjectInfoPath), Is.False);

                AssertApplicationTablesDoNotExist(dataSource);
            }
        }
        finally
        {
            migrationService.Migrate();
        }
    }

    private static Project InsertProject(IServiceScope scope, string projectPath, string projectName)
    {
        IProjectRepository projectRepository =
            scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        Project project = CreateProject(name: projectName, path: projectPath);

        return projectRepository.Insert(project);
    }

    private string CreateProjectInfoFile(string projectFolderName, int projectId)
    {
        return CreateFile(
            Path.Combine(
                projectFolderName,
                ProjectInfoFile),
            projectId.ToString());
    }

    private string GetProjectInfoPath(string projectFolderName)
    {
        return GetTemporaryPath(
            projectFolderName,
            ProjectInfoFile);
    }

    private static void AssertApplicationTablesExist(NpgsqlDataSource dataSource)
    {
        using (Assert.EnterMultipleScope())
        {
            foreach (string tableName in ApplicationTables)
            {
                Assert.That(
                    TableExists(dataSource, tableName),
                    Is.True,
                    $"Table should exist: {tableName}");
            }
        }
    }

    private static void AssertApplicationTablesDoNotExist(NpgsqlDataSource dataSource)
    {
        using (Assert.EnterMultipleScope())
        {
            foreach (string tableName in ApplicationTables)
            {
                Assert.That(
                    TableExists(dataSource, tableName),
                    Is.False,
                    $"Table should not exist: {tableName}");
            }
        }
    }

    private static bool TableExists(NpgsqlDataSource dataSource, string tableName)
    {
        using NpgsqlCommand command = dataSource.CreateCommand("""
                                                               SELECT to_regclass(@TableName) IS NOT NULL
                                                               """);

        command.Parameters.AddWithValue("TableName", $"public.{tableName}");

        return (bool) command.ExecuteScalar()!;
    }
}