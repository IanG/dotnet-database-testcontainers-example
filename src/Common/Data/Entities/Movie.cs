namespace TestingContainersExample.Common.Data.Entities;

public class Movie
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int YearOfRelease { get; set; }

    public DateTime CreatedAt { get; set; }
}
