using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestingContainersExample.Common.Services;

public static class ServiceBuilderExtensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMoviesService, MoviesService>();
    }
}