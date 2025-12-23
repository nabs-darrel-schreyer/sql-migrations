namespace SqlMigrations.MigrationCli.Commands;

public class DropDbSettings : NabsMigrationsSettings
{
    [Description("Name of the DbContext whose database should be dropped. Required when using the command line.")]
    [CommandOption("--context")]
    public string? Context { get; init; }

    /// <summary>
    /// Determines if the command should run in interactive mode.
    /// Interactive mode is used when the Context option is not provided.
    /// </summary>
    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Context);
}

internal sealed class DropDbCommand : Command<DropDbSettings>
{
    protected override int Execute(CommandContext context, DropDbSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[red]DROP DATABASE[/]");
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

    private int ExecuteCommandLineMode(DropDbSettings settings)
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

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start($"[green]Dropping DB:[/] [blue]{databaseName}[/]", ctx =>
            {
                var deleteResult = dbContext.Database.EnsureDeleted();
                if (deleteResult)
                {
                    AnsiConsole.MarkupLine($"[green]Successfully deleted DB:[/] [blue]{databaseName}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]DB did not exist or could not be deleted:[/] [blue]{databaseName}[/]");
                }
            });

        SolutionScanner.Unload();

        return 0;
    }

    private int ExecuteInteractiveMode(DropDbSettings settings)
    {
        SolutionScanner.Scan(settings.ScanPath);

        var dbContextItems = SolutionScanner
                    .Solution!
                    .ProjectItems
                    .SelectMany(p => p.DbContextFactoryItems)
                    .ToList();

        if (!dbContextItems.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No DbContexts were found in the solution.[/]");
            SolutionScanner.Unload();
            return 0;
        }

        // Build a list of all database info, grouped by connection string
        // Store DbContextFactoryItem instead of DbContext to avoid AssemblyLoadContext lifetime issues
        var allDbContextInfos = new List<(string ConnectionString, string ServerName, string DatabaseName, string ContextName, string Schema, DbContextFactoryItem FactoryItem)>();

        foreach (var dbContextItem in dbContextItems)
        {
            using var dbContext = SolutionScanner.CreateDbContext(dbContextItem)!;
            var connection = dbContext.Database.GetDbConnection();
            var connectionString = connection.ConnectionString;
            var schema = dbContext.Model.GetDefaultSchema() ?? "dbo";

            allDbContextInfos.Add((
                connectionString,
                connection.DataSource,
                connection.Database,
                dbContextItem.DbContextTypeName.Split('.').Last(),
                schema,
                dbContextItem));
        }

        // Group by connection string to identify unique databases
        var databaseGroups = allDbContextInfos
            .GroupBy(x => x.ConnectionString, StringComparer.OrdinalIgnoreCase)
            .ToList();

        AnsiConsole.MarkupLine($"The following [blue]{databaseGroups.Count}[/] database(s) are available to drop:");

        var table = new Table()
            .AddColumn("Server")
            .AddColumn("Database")
            .AddColumn("DbContext")
            .AddColumn("Schema");

        foreach (var group in databaseGroups)
        {
            var first = group.First();
            var isFirstRow = true;

            foreach (var dbInfo in group)
            {
                table.AddRow(
                    isFirstRow ? first.ServerName : string.Empty,
                    isFirstRow ? first.DatabaseName : string.Empty,
                    dbInfo.ContextName,
                    dbInfo.Schema);
                isFirstRow = false;
            }
        }
        AnsiConsole.Write(table);

        // Process each unique database (use the first DbContext from each group)
        foreach (var group in databaseGroups)
        {
            var first = group.First();

            var confirmation = AnsiConsole.Prompt(
                new TextPrompt<bool>($"Are you sure you want to [red]DROP[/] the database [blue]{first.ServerName}/{first.DatabaseName}[/]?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(false)
                    .WithConverter(choice => choice ? "y" : "n"));

            if (!confirmation)
            {
                AnsiConsole.MarkupLine($"[yellow]Skipping DB drop for:[/] [blue]{first.DatabaseName}[/]");
                continue;
            }

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start($"[green]Dropping DB:[/] [blue]{first.DatabaseName}[/]", ctx =>
                {
                    using var dbContext = SolutionScanner.CreateDbContext(first.FactoryItem)!;
                    var deleteResult = dbContext.Database.EnsureDeleted();
                    if (deleteResult)
                    {
                        AnsiConsole.MarkupLine($"[green]Successfully deleted DB:[/] [blue]{first.DatabaseName}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[yellow]DB did not exist or could not be deleted:[/] [blue]{first.DatabaseName}[/]");
                    }
                });
        }

        SolutionScanner.Unload();

        return 0;
    }
}
