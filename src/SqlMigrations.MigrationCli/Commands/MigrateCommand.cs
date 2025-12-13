namespace SqlMigrations.MigrationCli.Commands;

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
        var rule = new Rule("[yellow]MIGRATE DATABASES[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

        ProjectScanner.Scan(settings.ScanPath);

        var dbContextItems = ProjectScanner
                    .Solution!
                    .ProjectItems
                    .SelectMany(p => p.DbContextItems)
                    .ToList();

        foreach (var dbContextItem in dbContextItems)
        {
            using var dbContext = dbContextItem.CreateDbContext()!;
            var databaseName = dbContext.Database.GetDbConnection().Database;

            var confirmation = AnsiConsole.Prompt(
                new TextPrompt<bool>($"Are you sure you want to [yellow]MIGRATE[/] the database [blue]{databaseName}[/]?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(false)
                    .WithConverter(choice => choice ? "y" : "n"));

            if (!confirmation)
            {
                AnsiConsole.MarkupLine($"[yellow]Skipping DB migration for:[/] [blue]{databaseName}[/]");
                continue;
            }

            foreach (var pendingMigration in dbContext.Database.GetPendingMigrations())
            {
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start($"[green]Migrating DB:[/] [blue]{databaseName}[/]", ctx =>
                    {
                        try
                        {
                            dbContext.Database.Migrate(pendingMigration);
                            AnsiConsole.MarkupLine($"[green]Successfully migrated DB:[/] [blue]{databaseName}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[yellow]DB did not exist or could not be migrated:[/] [blue]{databaseName}[/]");
                            AnsiConsole.Markup(ex.StackTrace ?? string.Empty);
                        }
                    });
            }
        }

        MigrationsTree.Init();

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
