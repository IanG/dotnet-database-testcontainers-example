using Microsoft.AspNetCore.Mvc;
using TestingContainersExample.API.DTO;
using TestingContainersExample.Common.Data.Entities;
using TestingContainersExample.Common.Services;

namespace TestingContainersExample.API.Controllers;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly ILogger<MoviesController> _logger;
    private readonly IMoviesService _moviesService;

    public MoviesController(ILogger<MoviesController> logger, IMoviesService moviesService)
    {
        _logger = logger;
        _moviesService = moviesService;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Movie>>> GetMovies()
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("GetMovies called");

            IList<Movie> movies = await _moviesService.GetMovies();
            
            if (movies.Count > 0) return Ok(movies);
    
            return NoContent();
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Error fetching movies {exceptionMessage}", ex.Message);
            }
            
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching movies.");
        }
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Movie>> GetMovieById(int id)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("GetMovieById called with {id}", id);

            Movie? movie = await _moviesService.GetMovie(id);
    
            if (movie is not null) return Ok(movie);
    
            return NotFound();
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Error fetching Movie {id}. {exceptionMessage}", id, ex.Message);
            }
            
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while fetching Movie '{id}'.");
        }
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Movie>> CreateMovie([FromBody] CreateMovieRequest request)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("CreateMovie called");

            Movie? movie = await _moviesService.AddMovie(request.Name, request.YearOfRelease);

            if (movie is not null)
            {
                return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);    
            }

            return BadRequest();
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Error creating movie {exceptionMessage}", ex.Message);
            }
            
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while creating the movie");
        }
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteMovie([FromRoute] int id)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("Deleting movie {id}", id);

            if (await _moviesService.DeleteMovie(id))
            {
                return Ok();
            }
            
            return NotFound();
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Error deleting movie {id} {exceptionMessage}", id, ex.Message);
            }
            
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting movie {id}");
        }
    }
}