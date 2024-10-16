using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TestingContainersExample.Common.Data;

namespace TestingContainersExample.Tests.Integration.Fixtures;

public class MoviesDbContextFixture : IAsyncLifetime, IClassFixture<MoviesDbContextFixture>
{
    private readonly PostgreSqlContainer _postgresContainer;
    
    public MoviesDbContextFixture()
    {
        string dockerEntryPointInitDbDir =
            Path.Combine(Directory.GetCurrentDirectory(), "scripts", "docker-entrypoint-initdb.d");
        
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithUsername("moviesuser")
            .WithPassword("moviesuserpassword")
            .WithDatabase("movies")
            .WithPortBinding(5432, assignRandomHostPort: true)
            .WithBindMount(dockerEntryPointInitDbDir, "/docker-entrypoint-initdb.d")
            .Build();
    }

    public async Task InitializeAsync() => await _postgresContainer.StartAsync();
    public Task DisposeAsync() => _postgresContainer.StopAsync();

    public MoviesDbContext CreateMoviesDbContext()
    {
        DbContextOptions<MoviesDbContext> options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new MoviesDbContext(options);
    }
}