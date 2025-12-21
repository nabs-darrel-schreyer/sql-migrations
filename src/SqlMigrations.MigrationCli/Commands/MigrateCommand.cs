namespace SqlMigrations.MigrationCli.Commands;

internal sealed class MigrateCommand : Command<NabsMigrationsSettings>
{
    public class MigrateCommandSettings: NabsMigrationsSettings
    {
        // Additional settings specific to the Migrate command can be added here in the future
        
    }

    protected override int Execute(CommandContext context, NabsMigrationsSettings settings, CancellationToken cancellationToken)
    {

        var rule = new Rule("[yellow]MIGRATE DATABASES[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

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

        SolutionScanner.Unload();

        return 0;
    }
}
