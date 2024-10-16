using Microsoft.EntityFrameworkCore;
using TestingContainersExample.Common.Data.Entities;

namespace TestingContainersExample.Common.Data;

public partial class MoviesDbContext : DbContext
{
    public MoviesDbContext() { }

    public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options) { }

    public virtual DbSet<Movie> Movies { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("movies_pkey");

            entity.ToTable("movies");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at")
                .IsRequired();
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name")
                .IsRequired();
            entity.Property(e => e.YearOfRelease).HasColumnName("year_of_release");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
