using Microsoft.EntityFrameworkCore;

namespace SqlMigrations.MigrationCli.Commands;

public class ApplyMigrationsSettings : NabsMigrationsSettings
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

internal sealed class ApplyMigrationsCommand : AsyncCommand<ApplyMigrationsSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ApplyMigrationsSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]APPLY MIGRATIONS[/]");
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

            return await ExecuteCommandLineMode(settings);
        }

        return await ExecuteInteractiveMode(settings);
    }

    private async Task<int> ExecuteCommandLineMode(ApplyMigrationsSettings settings)
    {
        SolutionScanner.Scan(settings.ScanPath);

        await ProcessHelpers.BuildSolutionAsync();

        var dbContextItems = SolutionScanner
            .Solution!
            .ProjectItems
            .SelectMany(p => p.DbContextFactoryItems)
            .ToList();

        // Find the specified DbContext
        var targetDbContextItem = dbContextItems.FirstOrDefault(d =>
            d.DbContextTypeName.Split('.').Last()
                .Equals(settings.Context, StringComparison.OrdinalIgnoreCase));

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
                        AnsiConsole.Write(ex.Message);
                        AnsiConsole.Write(ex.StackTrace ?? "No stack trace");
                    }
                });
        }

        MigrationsTree.Init();
        SolutionScanner.Unload();

        return 0;
    }

    private async Task<int> ExecuteInteractiveMode(ApplyMigrationsSettings settings)
    {
        SolutionScanner.Scan(settings.ScanPath);

        var dbContextItems = SolutionScanner
                    .Solution!
                    .ProjectItems
                    .SelectMany(p => p.DbContextFactoryItems)
                    .ToList();

        var dbContextsWithMigrations = dbContextItems
            .Where(d => d.MigrationItems.Any())
            .ToList();

        if(!dbContextsWithMigrations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No DbContexts with migrations were found in the solution.[/]");
            SolutionScanner.Unload();
            return 0;
        }

        AnsiConsole.MarkupLine($"The following [blue]{dbContextsWithMigrations.Count}[/] migrations are available:");

        var table = new Table()
                    .AddColumn("Server")
                    .AddColumn("Database")
                    .AddColumn("DbContext")
                    .AddColumn("Pending Migration");

        foreach (var dbContextItem in dbContextItems)
        {
            using var dbContext = SolutionScanner.CreateDbContext(dbContextItem)!;
            var databaseName = dbContext.Database.GetDbConnection().Database;
            var databaseInstance = dbContext.Database.GetDbConnection().DataSource;
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();

            foreach (var pendingMigration in pendingMigrations)
            {
                table.AddRow(databaseInstance, databaseName, dbContextItem.DbContextTypeName.Split('.').Last(), pendingMigration);
            }
        }
        AnsiConsole.Write(table);

        foreach (var dbContextItem in dbContextItems)
        {
            using var dbContext = SolutionScanner.CreateDbContext(dbContextItem)!;
            var databaseName = dbContext.Database.GetDbConnection().Database;
            var databaseInstance = dbContext.Database.GetDbConnection().DataSource;
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();

            foreach (var pendingMigration in pendingMigrations)
            {
                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>($"Are you sure you want to [yellow]APPLY MIGRATION[/]: {pendingMigration}?")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(false)
                        .WithConverter(choice => choice ? "y" : "n"));

                if (!confirmation)
                {
                    AnsiConsole.MarkupLine($"[yellow]Skipping migration:[/] [blue]{pendingMigration}[/]");
                    continue;
                }

                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start($"[green]Applying migration:[/] [blue]{pendingMigration}[/]", ctx =>
                    {
                        try
                        {
                            dbContext.Database.Migrate(pendingMigration);
                            AnsiConsole.MarkupLine($"[green]Successfully applied migration:[/] [blue]{pendingMigration}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[yellow]Failed to apply migration:[/] [blue]{pendingMigration}[/]");
                            AnsiConsole.Write(ex.Message);
                            AnsiConsole.Write(ex.StackTrace ?? "No stack trace");
                        }
                    });
            }
        }

        MigrationsTree.Init();

        SolutionScanner.Unload();

        return 0;
    }
}
