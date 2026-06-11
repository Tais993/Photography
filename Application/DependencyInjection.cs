using Application.services;
using Application.services.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLogic()
        {
            services.AddScoped<ICopyService, CopyService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ILightroomService, LightroomService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IProjectInitialisingService, ProjectInitialisingService>();
            services.AddScoped<IProjectMetadataService, ProjectMetadataService>();

            services.AddScoped<IIrfanViewService, IrfanViewService>();
            services.AddScoped<IImageSelectionService, ImageSelectionService>();

            services.AddScoped<IThumbnailService, ThumbnailService>();
            return services;
        }
    }
}