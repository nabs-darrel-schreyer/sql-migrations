using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SqlMigrations.Persistence;

namespace SqlMigrations.DataMigrations;

public class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    const string connectionString = "Server=.;Database=SqlMigrationsDatabase;Integrated Security=True;TrustServerCertificate=True;";

    public TestDbContext CreateDbContext(string[] args)
    {
        var thisAssembly = typeof(TestDbContextFactory).Assembly;
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsAssembly(thisAssembly));
        return new TestDbContext(optionsBuilder.Options);
    }
}
