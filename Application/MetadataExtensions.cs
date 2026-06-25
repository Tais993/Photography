using Application.interfaces.services.metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application;

public static class MetadataExtensions
{
    extension(IHost app)
    {
        public void EnsureMetadataExists()
        {
            IServiceScope serviceScope = app.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            provider.GetRequiredService<IMetadataInitialisationService>().EnsureRequiredMetadataExists();
        }
    }   
}