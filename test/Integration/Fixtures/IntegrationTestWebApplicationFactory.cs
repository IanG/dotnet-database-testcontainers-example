using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using TestingContainersExample.Common.Data;

namespace TestingContainersExample.Tests.Integration.Fixtures;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime, IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly PostgreSqlContainer _moviesDatabaseContainer;

    public IntegrationTestWebApplicationFactory()
    {
        string dockerEntryPointInitDbDir =
            Path.Combine(Directory.GetCurrentDirectory(), "scripts", "docker-entrypoint-initdb.d");
        
        _moviesDatabaseContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithUsername("moviesuser")
            .WithPassword("moviesuserpassword")
            .WithDatabase("movies")
            .WithPortBinding(5432, assignRandomHostPort: true)
            .WithBindMount(dockerEntryPointInitDbDir, "/docker-entrypoint-initdb.d")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            Type type = typeof(DbContextOptions<MoviesDbContext>);
            ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == type);

            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<MoviesDbContext>(options => options
                .UseNpgsql(_moviesDatabaseContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync() => await _moviesDatabaseContainer.StartAsync();
    public new Task DisposeAsync() => _moviesDatabaseContainer.StopAsync();
}