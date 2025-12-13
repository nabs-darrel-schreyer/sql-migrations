namespace SqlMigrations.MigrationCli.Commands;

internal sealed class ResetCommand : Command<ResetCommand.ResetDbSettings>
{
    public sealed class ResetDbSettings : CommandSettings
    {
        [Description("Path to scan. Defaults to current directory.")]
        [CommandArgument(0, "[searchPath]")]
        public string? ScanPath { get; init; }
    }

    protected override int Execute(CommandContext context, ResetDbSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[red]DROP DATABASE[/]");
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

        return 0;
    }
}
