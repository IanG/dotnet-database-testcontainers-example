using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using TestingContainersExample.Common.Data;
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
    private readonly MoviesDbContext _moviesDbContext;
    private readonly FakeLogger<MoviesService> _logger;
    
    public MoviesServiceTests(MoviesDbContextFixture fixture)
    {
        _fixture = fixture;
        _logger = new FakeLogger<MoviesService>();
        _moviesDbContext = _fixture.CreateMoviesDbContext();
        _sut = new MoviesService(_logger, _moviesDbContext);
    }
    
    [Fact(DisplayName = "GetMovies - Should return Movies"), Priority(1)]
    [Trait("Category", "Service")]
    public async Task GetMoviesShouldReturnAllMovies()
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
    [Trait("Category", "Service")]
    public async Task GetMovieForExistingMovieShouldReturnMovie()
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
    [Trait("Category", "Service")]
    public async Task AddMovieShouldAddMovie()
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
    [Trait("Category", "Service")]
    public async Task DeleteMovieShouldDeleteMovie()
    {
        int expectedMovieCountPostDelete = _fixture.CreateMoviesDbContext().Movies.Count() - 1;

        bool deleted = await _sut.DeleteMovie(3);
        
        int curentMovieCount = _fixture.CreateMoviesDbContext().Movies.Count();
        
        deleted.Should().BeTrue();
        curentMovieCount.Should().Be(expectedMovieCountPostDelete);
    }
    
    [Fact(DisplayName = "DeleteMovie - Deleting a movie that doesn't exist should not change the DB"), Priority(4)]
    [Trait("Category", "Service")]
    public async Task DeleteMovieThatDoesNotExistShouldReturnFalse()
    {
        int expectedMovieCountPostDelete = _fixture.CreateMoviesDbContext().Movies.Count();

        bool deleted = await _sut.DeleteMovie(10);
        
        int curentMovieCount = _fixture.CreateMoviesDbContext().Movies.Count();

        deleted.Should().BeFalse();
        curentMovieCount.Should().Be(expectedMovieCountPostDelete);
    }
    
    [Fact (DisplayName = "DeleteMovie - When an exception is thrown it should log the error and return false"), Priority(5)]
    [Trait("Category", "Service")]
    public async Task DeleteMovieWhenExceptionThrownShouldLogErrorAndReturnFalse()
    {
        int movieId = 666;
        
        // Intentionally kill the db context to trigger an exception
        await _moviesDbContext.DisposeAsync();
        
        bool result = await _sut.DeleteMovie(movieId);
    
        
        result.Should().BeFalse();
        
        IReadOnlyList<FakeLogRecord> logEntries = _logger.Collector.GetSnapshot();
        
        logEntries.Should().NotBeEmpty();
        logEntries.Should().HaveCount(2);

        logEntries.First().Level.Should().Be(LogLevel.Debug);
        logEntries.First().Message.Should().Be($"Deleting Movie {movieId}");
        logEntries.Last().Level.Should().Be(LogLevel.Error);
        logEntries.Last().Message.Should().StartWith($"Error deleting movie {movieId}");
    }

    [Fact(DisplayName = "AddMovie - When an exception is thrown it should log the error and return default"), Priority(6)]
    [Trait("Category", "Service")]
    public async Task AddMovieWhenExceptionThrownShouldLogErrorAndReturnTrue()
    {
        string newMovieName = "A New Movie";
        int newMovieYearOfRelease = 2024;
        
        await _moviesDbContext.DisposeAsync();
        Movie? addedMovie = await _sut.AddMovie(newMovieName, newMovieYearOfRelease);

        addedMovie.Should().BeNull();
        
        IReadOnlyList<FakeLogRecord> logEntries = _logger.Collector.GetSnapshot();
        
        logEntries.Should().NotBeEmpty();
        logEntries.Should().HaveCount(2);
        logEntries.First().Level.Should().Be(LogLevel.Debug);
        logEntries.First().Message.Should().Be($"Adding Movie {newMovieName} {newMovieYearOfRelease}");
        logEntries.Last().Level.Should().Be(LogLevel.Error);
    }
}