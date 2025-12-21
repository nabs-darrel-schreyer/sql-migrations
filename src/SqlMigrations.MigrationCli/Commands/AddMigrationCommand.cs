namespace SqlMigrations.MigrationCli.Commands;

internal sealed class AddMigrationCommand : AsyncCommand<NabsMigrationsSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, NabsMigrationsSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]ADD NEW MIGRATIONS[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

        SolutionScanner.Scan(settings.ScanPath);

        var projectItems = SolutionScanner
                    .Solution!
                    .ProjectItems
                    .ToList();

        var hasAnyOutstandingChanges = projectItems
            .SelectMany(pi => pi.DbContextFactoryItems)
            .Any(dfi => dfi.PendingModelChanges.Count > 0);

        if (!hasAnyOutstandingChanges)
        {
            AnsiConsole.MarkupLine("[green]No outstanding changes detected in any DbContext. No migrations to add.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine("[yellow]The following DbContext(s) have outstanding changes that require new migrations:[/]");

        foreach (var projectItem in projectItems)
        {
            foreach (var dbContextFactoryItem in projectItem.DbContextFactoryItems)
            {
                if (dbContextFactoryItem.PendingModelChanges.Count == 0)
                {
                    continue;
                }

                AnsiConsole.MarkupLine($"[blue]'{dbContextFactoryItem.DbContextTypeName}':[/]");

                foreach (var outstandingChange in dbContextFactoryItem.PendingModelChanges)
                {
                    var changeType = outstandingChange.IsDestructive
                        ? "[red]DESTRUCTIVE[/]"
                        : "[green]NON-DESTRUCTIVE[/]";

                    AnsiConsole.MarkupLine($"  * {outstandingChange.Description} ({changeType})");
                }


                var dbContextName = dbContextFactoryItem.DbContextTypeName;
                var projectName = projectItem.ProjectFile.Name.Replace(".csproj", "");

                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>($"Are you sure you want to [red]ADD A MIGRATION[/] for [blue]{dbContextName}[/]?")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(false)
                        .WithConverter(choice => choice ? "y" : "n"));

                if (!confirmation)
                {
                    AnsiConsole.MarkupLine($"[yellow]Skipping ADD MIGRATION for[/] [blue]{dbContextName}[/]");
                    continue;
                }

                var migrationName = AnsiConsole.Prompt(
                    new TextPrompt<string>($"Enter the name for the new migration:")
                        .Validate(name =>
                        {
                            return string.IsNullOrWhiteSpace(name)
                                ? ValidationResult.Error("[red]Migration name cannot be empty[/]")
                                : ValidationResult.Success();
                        }));

                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .StartAsync($"[green]Adding migration[/] [blue]{migrationName}[/] to [blue]{dbContextName}[/]", async ctx =>
                    {
                        var dbContextName = dbContextFactoryItem.DbContextTypeName.Split('.').Last();

                        try
                        {
                            await ProcessHelpers.RunProcessAsync(
                                "dotnet",
                                $"ef migrations add {migrationName} --context {dbContextName} --output-dir Migrations/{dbContextName}Migrations --verbose",
                                projectItem.ProjectFile.Directory!.FullName);

                            AnsiConsole.MarkupLine($"[green]Successfully added new migration:[/] [blue]{migrationName}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Failed to add migration to:[/] [blue]{migrationName}[/]");
                            AnsiConsole.Markup(ex.StackTrace!);
                        }
                    });
            }
        }

        return 0;
    }
}
