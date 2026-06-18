using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration.Fixtures;

[Category("Integration")]
public abstract class IntegrationTestBase
{
    private static readonly PostgreSqlTestFixture Fixture = new();


    protected IServiceProvider Services => Fixture.Services;
    protected string FixtureConnectionString => Fixture.ConnectionString;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await Fixture.StartAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await Fixture.ResetDatabaseAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Fixture.DisposeAsync();
    }

    protected IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }
}