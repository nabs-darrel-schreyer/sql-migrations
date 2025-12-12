using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace SqlMigrations.MigrationCli;

public sealed class MainViewModel(IServiceProvider serviceProvider)
{
    private const string Format = "yyyyMMddHHmmss";
    

    public void Init()
    {
        using var serviceScope = serviceProvider.CreateScope();

        //foreach (var dbContextFactoryItem in DbContextScanner.Items)
        //{
        //    var dbContext = dbContextFactoryItem.CreateDbContext();
        //    if (dbContext == null)
        //    {
        //        continue;
        //    }

        //    using (dbContext)
        //    {
        //        var appliedMigrations = dbContext.Database.GetAppliedMigrations();
        //        if (appliedMigrations.Any())
        //        {
        //            var projectName = Path.GetFileNameWithoutExtension(dbContextFactoryItem.ProjectPath);
        //            var dbContextName = dbContextFactoryItem.DbContextType.Name;

        //            foreach (var migration in appliedMigrations)
        //            {
        //                var parts = migration.Split('_');
        //                Migrations.Add(new MigrationItem
        //                {
        //                    Name = parts[1],
        //                    DbContextId = dbContextFactoryItem.Id,
        //                    AppliedOn = DateTime.ParseExact(parts[0], Format, CultureInfo.InvariantCulture),
        //                    DbContextTypeName = dbContextFactoryItem.DbContextTypeName,
        //                    ProjectName = projectName,
        //                    DbContextName = dbContextName
        //                });
        //            }
        //        }
        //    }
        //}
    }

    public void Exit()
    {
        Environment.Exit(0);
    }

    public void ResetDatabase(DbContextItem dbContextItem)
    {
        var dbContext = dbContextItem.CreateDbContext();
        dbContext?.Database.EnsureDeleted();
        dbContext?.Database.EnsureCreated();
    }

    public void RevertMigration(DbContextItem dbContextItem, MigrationItem migrationItem)
    {
        var dbContext = dbContextItem.CreateDbContext();
        if (dbContext == null)
        {
            return;
        }
        dbContext.Database.Migrate(migrationItem.FullName);
    }
}