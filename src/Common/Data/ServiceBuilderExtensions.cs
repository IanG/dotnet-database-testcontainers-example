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
        string? connectionString = configuration.GetConnectionString("MoviesDb");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Could not find a connection string in configuration.");
        }
        
        services.AddDbContext<MoviesDbContext>(options => options.UseNpgsql(connectionString));
        
        services.AddHealthChecks().AddNpgSql(
            connectionString: connectionString,
            name:"MoviesDB",
            tags: ["db", "sql", "postgres"],
            timeout: TimeSpan.FromSeconds(10));
    }
}