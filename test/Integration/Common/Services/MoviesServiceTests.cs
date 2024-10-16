using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Logging.Testing;
using TestingContainersExample.Common.Data.Entities;
using TestingContainersExample.Common.Services;
using TestingContainersExample.Tests.Integration.Fixtures;
using Xunit.Priority;

namespace TestingContainersExample.Tests.Integration.Common.Services;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class MoviesServiceTests : IClassFixture<MoviesDbContextFixture>
{
    private readonly MoviesDbContextFixture _fixture;
    private readonly IMoviesService _sut;
    
    public MoviesServiceTests(MoviesDbContextFixture fixture)
    {
        _fixture = fixture;
        _sut = new MoviesService(new FakeLogger<MoviesService>(), _fixture.CreateMoviesDbContext());
    }
    
    [Fact(DisplayName = "GetMovies - Should return Movies"), Priority(1)]
    public async void GetMoviesShouldReturnAllMovies()
    {
        IList<Movie> expectedMovies = new List<Movie>
        {
            new() { Id = 1, Name = "Gone With The Wind", YearOfRelease = 1939, CreatedAt = new DateTime(2024, 10, 10, 10, 10, 10) },
            new() { Id = 2, Name = "Back To The Future", YearOfRelease = 1985, CreatedAt = new DateTime(2024, 10, 10, 10, 10, 10) }
        };
        
        IList<Movie> movies = await _sut.GetMovies();
        
        movies.Should().NotBeNullOrEmpty();
        movies.Should().HaveCount(expectedMovies.Count);
        movies.Should().BeEquivalentTo(expectedMovies);
    }

    [Fact(DisplayName = "GetMovie - Getting movie with Id 1 Should return Movie with Id"), Priority(2)]
    public async void GetMovieForExistingMovieShouldReturnMovie()
    {
        Movie expectedMovie = new Movie()
        {
            Id = 1,
            Name = "Gone With The Wind",
            YearOfRelease = 1939,
            CreatedAt = new DateTime(2024, 10, 10, 10, 10, 10)
        };
        
        Movie? movie = await _sut.GetMovie(1);

        movie.Should().NotBeNull();
        movie.Should().BeEquivalentTo(expectedMovie);
    }

    [Fact(DisplayName = "AddMovie - Adding a new movie should add the movie"), Priority(3)]
    public async void AddMovieShouldAddMovie()
    {
        int expectedMovieCount = 3;
        
        Movie expectedMovie = new Movie()
        {
            Id = 3,
            Name = "Topgun",
            YearOfRelease = 1986,
            CreatedAt = DateTime.UtcNow
        };
        
        Movie? movie = await _sut.AddMovie(expectedMovie.Name, expectedMovie.YearOfRelease);
        
        int movieCount = _fixture.CreateMoviesDbContext().Movies.Count();
        
        movie.Should().NotBeNull();
        movie.Should().BeEquivalentTo(expectedMovie, options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
                .When(info => info.Path == "CreatedAt"));
        
        movieCount.Should().Be(expectedMovieCount);
    }

    [Fact(DisplayName = "DeleteMovie - When Deleting a movie it should be removed from the DB"), Priority(4)]
    public async void DeleteMovieShouldDeleteMovie()
    {
        int expectedMovieCountPostDelete = _fixture.CreateMoviesDbContext().Movies.Count() - 1;

        bool deleted = await _sut.DeleteMovie(3);
        
        int curentMovieCount = _fixture.CreateMoviesDbContext().Movies.Count();
        
        deleted.Should().BeTrue();
        curentMovieCount.Should().Be(expectedMovieCountPostDelete);
    }
}