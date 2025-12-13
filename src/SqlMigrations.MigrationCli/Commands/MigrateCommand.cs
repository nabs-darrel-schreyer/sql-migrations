using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using Spectre.Console.Cli;
using SqlMigrations.MigrationCli;
using System.ComponentModel;

internal sealed class MigrateCommand : Command<MigrateCommand.MigrateSettings>
{
    public sealed class MigrateSettings : CommandSettings
    {
        [Description("Path to scan. Defaults to current directory.")]
        [CommandArgument(0, "[searchPath]")]
        public string? ScanPath { get; init; }

        [Description("Filter by project name.")]
        [CommandOption("-a|--action")]
        public string? Action { get; init; }
    }

    protected override int Execute(CommandContext context, MigrateSettings settings, CancellationToken cancellationToken)
    {
        ProjectScanner.Scan(settings.ScanPath);

        MigrationsTree.Init();

        AnsiConsole.Live(new Panel(""))
            .Start(ctx =>
            {
                var dbContextItems = ProjectScanner
                    .Solution!
                    .ProjectItems
                    .SelectMany(p => p.DbContextItems)
                    .ToList();

                foreach (var dbContextItem in dbContextItems)
                {
                    using var dbContext = dbContextItem.CreateDbContext();

                    foreach (var pendingMigration in dbContext.Database.GetPendingMigrations())
                    {
                        AnsiConsole.MarkupLine($"[yellow]Pending migration found:[/] [blue]{pendingMigration}[/]");
                        dbContext.Database.Migrate(pendingMigration);
                    }
                }

                //Thread.Sleep(500);
                ctx.Refresh();
            });
        
        return 0;
    }
}

//    // Check if there are any migrations at all
//    var allMigrations = dbContext.Database.GetMigrations();
//    if (!allMigrations.Any())
//    {
//        Console.WriteLine("No migrations found in the assembly. Creating InitialCreate ...");
//        await dbContext.AddMigration("InitialCreate");
//        return;
//    }
//}

//await using (var serviceScope = serviceProvider.CreateAsyncScope())
//{
//    await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<TestDbContext>();

//    var hasPendingModelChanges = dbContext.Database.HasPendingModelChanges();
//    if (hasPendingModelChanges)
//    {
//        var pendingModelChanges = dbContext.GetPendingModelChanges();
//        foreach (var change in pendingModelChanges)
//        {
//            Console.WriteLine($"Found pending model change: {change}");
//        }
//    }
//}

//await using (var serviceScope = serviceProvider.CreateAsyncScope())
//{
//    await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<TestDbContext>();

//    var allMigrations = dbContext.Database.GetMigrations();
//    foreach (var migration in allMigrations)
//    {
//        Console.WriteLine($"Found migration: {migration}");
//    }

//    var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
//    foreach (var migration in appliedMigrations)
//    {
//        Console.WriteLine($"Applied migration: {migration}");
//    }



//    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
//    if (pendingMigrations.Any())
//    {
//        Console.WriteLine("Applying pending migrations...");
//        await dbContext.Database.MigrateAsync();
//        Console.WriteLine("Migrations applied successfully.");
//    }
//    else
//    {
//        Console.WriteLine("No pending migrations found.");
//    }

//}
