namespace SqlMigrations.MigrationCli.Commands;

internal sealed class ResetAllMigrationsCommand : AsyncCommand<NabsMigrationsSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, NabsMigrationsSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]RESET MIGRATIONS[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

        SolutionScanner.Scan(settings.ScanPath);

        var projectItems = SolutionScanner
                    .Solution!
                    .ProjectItems
                    .ToList();

        var hasAnyMigrations = projectItems
            .SelectMany(pi => pi.DbContextFactoryItems)
            .Any(dfi => dfi.MigrationItems.Count > 0);

        if (!hasAnyMigrations)
        {
            AnsiConsole.MarkupLine("[green]No migrations found to reset.[/]");
            return 0;
        }

        foreach (var projectItem in projectItems)
        {
            foreach (var dbContextFactoryItem in projectItem.DbContextFactoryItems)
            {
                foreach (var migrationItem in dbContextFactoryItem.MigrationItems)
                {
                    var dbContextName = dbContextFactoryItem.DbContextTypeName;

                    var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>($"Do you want to [red]DELETE ALL MIGRATIONs[/] for the DbContext: [blue]{dbContextName}[/]?")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(false)
                        .WithConverter(choice => choice ? "y" : "n"));

                    if (!confirmation)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Skipping DELETE ALL MIGRATION for:[/] [blue]{dbContextName}[/]");
                        continue;
                    }

                    await AnsiConsole.Status()
                        .Spinner(Spinner.Known.Dots)
                        .StartAsync($"[green]Deleting all migrations.[/]", async ctx =>
                        {
                            try
                            {
                                AnsiConsole.MarkupLine($"[green]Successfully added new migration:[/] [blue]{dbContextName}[/]");
                            }
                            catch (Exception ex)
                            {
                                AnsiConsole.MarkupLine($"[red]Failed to delete all migrations:[/] [blue]{dbContextName}[/]");
                                AnsiConsole.Markup(ex.StackTrace!);
                            }
                        });
                }
            }
        }

        return 0;
    }
}

