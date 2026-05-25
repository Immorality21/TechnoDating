using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Infrastructure.Entities;

namespace TechnoDating.Api.Infrastructure;

public class TechnoDatingDbContext(DbContextOptions<TechnoDatingDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Festival> Festivals => Set<Festival>();
    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Email).IsRequired().HasMaxLength(256);
            b.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
            b.Property(u => u.Gender).IsRequired().HasMaxLength(32);
            b.Property(u => u.Bio).HasMaxLength(2000);
            b.Property(u => u.City).IsRequired().HasMaxLength(100);
            b.Property(u => u.Location).HasColumnType("geography(Point, 4326)");
            b.Property(u => u.TopArtists).HasColumnType("text[]");
        });

        modelBuilder.Entity<Festival>(b =>
        {
            b.HasKey(f => f.Id);
            b.Property(f => f.Name).IsRequired().HasMaxLength(200);
            b.Property(f => f.City).IsRequired().HasMaxLength(100);
            b.Property(f => f.Venue).IsRequired().HasMaxLength(200);
            b.Property(f => f.HeadlineArtists).HasColumnType("text[]");
            b.Property(f => f.Location).HasColumnType("geography(Point, 4326)");
        });

        modelBuilder.Entity<Match>(b =>
        {
            b.HasKey(m => m.Id);
            b.HasIndex(m => new { m.UserAId, m.UserBId }).IsUnique();
            b.HasOne(m => m.UserA).WithMany().HasForeignKey(m => m.UserAId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(m => m.UserB).WithMany().HasForeignKey(m => m.UserBId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
