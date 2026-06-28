using Application.interfaces.services;
using Application.interfaces.services.metadata;
using Application.interfaces.services.project;
using Application.interfaces.website;
using Application.services;
using Application.services.metadata;
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
            
            services.AddScoped<IMetadataService, MetadataService>();
            services.AddScoped<IImageMetadataService, ImageMetadataService>();
            services.AddScoped<IProjectMetadataService, ProjectMetadataService>();
            services.AddScoped<IMetadataInitialisationService, MetadataInitialisationService>();
            services.AddScoped<ICollectionMetadataService, CollectionMetadataService>();
            
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectInfoFileService, ProjectInfoFileService>();
            services.AddScoped<IProjectResolverService, ProjectResolverService>();
            services.AddScoped<IProjectInitialisingService, ProjectInitialisingService>();
            services.AddScoped<IProjectScanningService, ProjectScanningService>();
            services.AddScoped<IProjectFolderService, ProjectFolderService>();
            
            
            services.AddScoped<IImageSelectionService, ImageSelectionService>();

            services.AddScoped<IThumbnailService, ThumbnailService>();

            services.AddScoped<ICameraDriveService, CameraDriveService>();
            
            // Website services
            services.AddScoped<ISelectionIndexService, SelectionIndexService>();
            services.AddScoped<IProjectIndexService, ProjectIndexService>();
            return services;
        }
    }
}