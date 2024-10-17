using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestingContainersExample.Common.Data;

[ExcludeFromCodeCoverage]
public static class ServiceBuilderExtensions
{
    public static void AddDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MoviesDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("MoviesDb")));
    }
}