namespace Website;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWebsite()
        {
            services.AddRazorPages();

            return services;
        }
    }
}