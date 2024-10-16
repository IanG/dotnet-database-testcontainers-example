using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestingContainersExample.Common.Data;
using TestingContainersExample.Common.Data.Entities;

namespace TestingContainersExample.Common.Services;

public class MoviesService : IMoviesService
{
    private readonly ILogger<MoviesService> _logger;
    private readonly MoviesDbContext _moviesDbContext;

    public MoviesService(ILogger<MoviesService> logger, MoviesDbContext? moviesDbContext)
    {
        _logger = logger;
        _moviesDbContext = moviesDbContext!;
    }

    public async Task<Movie?> GetMovie(int id)
    {
        if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("Getting Movie {id}", id);
        
        Movie? movie = await _moviesDbContext.Movies.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);
        
        return movie;
    }

    public async Task<IList<Movie>> GetMovies()
    {
        if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("Getting Movies");
        
        return await _moviesDbContext.Movies.ToListAsync();
    }

    public async Task<Movie?> AddMovie(string name, int yearOfRelease)
    {
        if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("Adding Movie {name} {yearOfRelease}", name, yearOfRelease);

        try
        {
            Movie movie = new Movie { Name = name, YearOfRelease = yearOfRelease };
        
            await _moviesDbContext.AddAsync(movie);
            await _moviesDbContext.SaveChangesAsync();

            return movie;
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Error adding movie {exceptionMessage}", ex.Message);
            }

            return default;
        }
    }

    public async Task<bool> DeleteMovie(int id)
    {
        if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("Deleting Movie {id}", id);
        
        try
        {
            Movie? movie = await _moviesDbContext.Movies.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id);

            if (movie is not null)
            {
                _moviesDbContext.Remove(movie);
                await _moviesDbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Error deleting movie {id} {exceptionMessage}", id, ex.Message);
            }

            return false;
        }
    }
}