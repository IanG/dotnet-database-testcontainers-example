using TestingContainersExample.Common.Data.Entities;

namespace TestingContainersExample.Common.Services;

public interface IMoviesService
{
    Task<Movie?> GetMovie(int id);
    Task<IList<Movie>> GetMovies();
    Task<Movie?> AddMovie(string name, int yearOfRelease);
    Task<bool> DeleteMovie(int id);
}