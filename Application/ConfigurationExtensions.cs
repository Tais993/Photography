using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Application;

public static class ConfigurationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddSharedConfiguration()
        {
            builder.Configuration.AddJsonFile(
                "appsettings.shared.json",
                optional: false,
                reloadOnChange: true
            );

            return builder;
        }
    }
}