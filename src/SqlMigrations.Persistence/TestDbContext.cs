using Microsoft.EntityFrameworkCore;
using SqlMigrations.Persistence.Entities;

namespace SqlMigrations.Persistence;

public sealed class TestDbContext(
    DbContextOptions<TestDbContext> options)
    : DbContext(options)
{
    public DbSet<PersonEntity> People { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("test");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDbContext).Assembly);
    }
}