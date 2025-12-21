namespace SqlMigrations.MigrationCli.Commands;

internal sealed class DropDbCommand : Command<NabsMigrationsSettings>
{
    protected override int Execute(CommandContext context, NabsMigrationsSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[red]DROP DATABASE[/]");
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
                new TextPrompt<bool>($"Are you sure you want to [red]DROP[/] the database [blue]{databaseName}[/]?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(false)
                    .WithConverter(choice => choice ? "y" : "n"));

            if (!confirmation)
            {
                AnsiConsole.MarkupLine($"[yellow]Skipping DB drop for:[/] [blue]{databaseName}[/]");
                continue;
            }

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
        }

        SolutionScanner.Unload();

        return 0;
    }
}
