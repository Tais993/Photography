using Application.interfaces.services;
using Application.interfaces.services.project;
using Application.interfaces.website;
using Application.services;
using Application.services.project;
using Application.website;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLogic()
        {
            services.AddScoped<IApplicationResetService, ApplicationResetService>();
            
            services.AddScoped<ICopyService, CopyService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ILightroomService, LightroomService>();
            services.AddScoped<IImageService, ImageService>();
            
            
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectResolverService, ProjectResolverService>();
            services.AddScoped<IProjectInitialisingService, ProjectInitialisingService>();
            services.AddScoped<IProjectMetadataService, ProjectMetadataService>();
            services.AddScoped<IProjectFolderService, ProjectFolderService>();
            
            services.AddScoped<IMetadataInitialisationService, MetadataInitialisationService>();
            services.AddScoped<ICollectionMetadataService, CollectionMetadataService>();
            
            
            services.AddScoped<IImageSelectionService, ImageSelectionService>();

            services.AddScoped<IThumbnailService, ThumbnailService>();
            
            // Website services
            services.AddScoped<ISelectionIndexService, SelectionIndexService>();
            services.AddScoped<IProjectIndexService, ProjectIndexService>();
            return services;
        }
    }
}