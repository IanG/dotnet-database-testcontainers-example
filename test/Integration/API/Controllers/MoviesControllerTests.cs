using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using TestingContainersExample.API.DTO;
using TestingContainersExample.Common.Data.Entities;
using TestingContainersExample.Tests.Integration.Fixtures;
using Xunit.Priority;

namespace TestingContainersExample.Tests.Integration.API.Controllers;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class MoviesControllerTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public MoviesControllerTests(IntegrationTestWebApplicationFactory factory)
    {  
        _factory = factory;
    }

    [Fact(DisplayName = "Get All Movies at /api/movies"), Priority(1)]
    [Trait("Category", "API")]
    public async Task GetAllMovies()
    {
        HttpClient client = _factory.CreateClient();
        
        List<Movie>? movies = await client.GetFromJsonAsync<List<Movie>>("api/movies");

        movies.Should().NotBeNullOrEmpty();
        movies!.Count.Should().Be(2);
    }

    [Fact(DisplayName = "Get Movie By Id at /api/movies/1"), Priority(2)]
    [Trait("Category", "API")]
    public async Task GetMovieById()
    {
        Movie expectedMovie = new Movie
        {
            Id = 1,
            Name = "Gone With The Wind",
            YearOfRelease = 1939,
            CreatedAt = new DateTime(2024, 10, 10, 10, 10, 10)
        };
        
        HttpClient client = _factory.CreateClient();
        
        HttpResponseMessage response = await client.GetAsync("/api/movies/1");
        
        response.IsSuccessStatusCode.Should().BeTrue();
        
        Movie? movie = JsonSerializer.Deserialize<Movie>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions);

        movie.Should().NotBeNull();
        movie.Should().BeEquivalentTo(expectedMovie);
    }

    [Fact(DisplayName = "Remove Movie by id at /api/movies/1"), Priority(3)]
    [Trait("Category", "API")]
    public async Task RemoveMovieById()
    {
        HttpClient client = _factory.CreateClient();
        
        HttpResponseMessage response = await client.DeleteAsync("/api/movies/1");
        
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Add a new movie at /api/movies")]
    [Trait("Category", "API")]
    public async Task AddMovie()
    {
        HttpClient client = _factory.CreateClient();

        CreateMovieRequest createMovieRequest = new CreateMovieRequest(Name: "Topgun", YearOfRelease: 1986);
        
        var content = new StringContent(JsonSerializer.Serialize(createMovieRequest), Encoding.UTF8, "application/json");
        
        HttpResponseMessage response = await client.PostAsync("/api/movies", content);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Movie movie = JsonSerializer.Deserialize<Movie>(await response.Content.ReadAsStringAsync(), _jsonSerializerOptions)!;
    
        movie.Should().NotBeNull();
        movie.Id.Should().BeGreaterThan(0);
        movie.Name.Should().Be(createMovieRequest.Name);
        movie.YearOfRelease.Should().Be(createMovieRequest.YearOfRelease);
        movie.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }
}