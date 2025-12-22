namespace SqlMigrations.MigrationCli.Commands;

public class ApplyMigrationSettings : NabsMigrationsSettings
{
    [Description("Name of the DbContext to migrate. Required when using the command line.")]
    [CommandOption("--context")]
    public string? Context { get; init; }

    [Description("Name of the specific migration to apply. If not provided, all pending migrations will be applied.")]
    [CommandOption("--migrationName")]
    public string? MigrationName { get; init; }

    /// <summary>
    /// Determines if the command should run in interactive mode.
    /// Interactive mode is used when the Context option is not provided.
    /// </summary>
    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Context);
}

internal sealed class ApplyMigrationCommand : Command<ApplyMigrationSettings>
{
    protected override int Execute(CommandContext context, ApplyMigrationSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]APPLY MIGRATION[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

        // Validate command line mode settings
        if (!settings.IsInteractiveMode)
        {
            if (string.IsNullOrWhiteSpace(settings.Context))
            {
                AnsiConsole.MarkupLine("[red]Error: --context is required when using command line mode.[/]");
                return 1;
            }

            return ExecuteCommandLineMode(settings);
        }

        return ExecuteInteractiveMode(settings);
    }

    private int ExecuteCommandLineMode(ApplyMigrationSettings settings)
    {
        SolutionScanner.Scan(settings.ScanPath);

        var dbContextItems = SolutionScanner
            .Solution!
            .ProjectItems
            .SelectMany(p => p.DbContextFactoryItems)
            .ToList();

        // Find the specified DbContext
        DbContextFactoryItem? targetDbContextItem = null;

        foreach (var dbContextFactoryItem in dbContextItems)
        {
            var contextName = dbContextFactoryItem.DbContextTypeName.Split('.').Last();
            if (contextName.Equals(settings.Context, StringComparison.OrdinalIgnoreCase))
            {
                targetDbContextItem = dbContextFactoryItem;
                break;
            }
        }

        if (targetDbContextItem == null)
        {
            AnsiConsole.MarkupLine($"[red]Error: DbContext '{settings.Context}' was not found in the solution.[/]");
            SolutionScanner.Unload();
            return 1;
        }

        using var dbContext = SolutionScanner.CreateDbContext(targetDbContextItem)!;
        var databaseName = dbContext.Database.GetDbConnection().Database;
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();

        if (pendingMigrations.Count == 0)
        {
            AnsiConsole.MarkupLine($"[green]No pending migrations for database:[/] [blue]{databaseName}[/]");
            SolutionScanner.Unload();
            return 0;
        }

        // If a specific migration name is provided, filter to only that migration
        if (!string.IsNullOrWhiteSpace(settings.MigrationName))
        {
            var targetMigration = pendingMigrations.FirstOrDefault(m =>
                m.EndsWith(settings.MigrationName, StringComparison.OrdinalIgnoreCase) ||
                m.Equals(settings.MigrationName, StringComparison.OrdinalIgnoreCase));

            if (targetMigration == null)
            {
                AnsiConsole.MarkupLine($"[red]Error: Migration '{settings.MigrationName}' was not found in pending migrations.[/]");
                SolutionScanner.Unload();
                return 1;
            }

            pendingMigrations = [targetMigration];
        }

        foreach (var pendingMigration in pendingMigrations)
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start($"[green]Applying migration[/] [blue]{pendingMigration}[/] to [blue]{databaseName}[/]", ctx =>
                {
                    try
                    {
                        dbContext.Database.Migrate(pendingMigration);
                        AnsiConsole.MarkupLine($"[green]Successfully applied migration:[/] [blue]{pendingMigration}[/]");
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Failed to apply migration:[/] [blue]{pendingMigration}[/]");
                        AnsiConsole.Markup(ex.StackTrace ?? string.Empty);
                    }
                });
        }

        MigrationsTree.Init();
        SolutionScanner.Unload();

        return 0;
    }

    private int ExecuteInteractiveMode(ApplyMigrationSettings settings)
    {
        SolutionScanner.Scan(settings.ScanPath);

        var dbContextItems = SolutionScanner
                    .Solution!
                    .ProjectItems
                    .SelectMany(p => p.DbContextFactoryItems)
                    .ToList();

        foreach (var dbContextItem in dbContextItems)
        {
            using var dbContext = SolutionScanner.CreateDbContext(dbContextItem)!;
            var databaseName = dbContext.Database.GetDbConnection().Database;

            var confirmation = AnsiConsole.Prompt(
                new TextPrompt<bool>($"Are you sure you want to [yellow]APPLY MIGRATION[/] to the database: [blue]{databaseName}[/]?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(false)
                    .WithConverter(choice => choice ? "y" : "n"));

            if (!confirmation)
            {
                AnsiConsole.MarkupLine($"[yellow]Skipping migration for:[/] [blue]{databaseName}[/]");
                continue;
            }

            foreach (var pendingMigration in dbContext.Database.GetPendingMigrations())
            {
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start($"[green]Migrating database:[/] [blue]{databaseName}[/]", ctx =>
                    {
                        try
                        {
                            dbContext.Database.Migrate(pendingMigration);
                            AnsiConsole.MarkupLine($"[green]Successfully migrated database:[/] [blue]{databaseName}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[yellow]The database did not exist or could not be migrated:[/] [blue]{databaseName}[/]");
                            AnsiConsole.Markup(ex.StackTrace ?? string.Empty);
                        }
                    });
            }
        }

        MigrationsTree.Init();

        SolutionScanner.Unload();

        return 0;
    }
}
