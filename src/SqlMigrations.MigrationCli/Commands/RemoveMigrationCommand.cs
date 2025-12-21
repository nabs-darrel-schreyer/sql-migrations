namespace SqlMigrations.MigrationCli.Commands;

internal sealed class RemoveMigrationCommand : AsyncCommand<NabsMigrationsSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, NabsMigrationsSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]REMOVE MIGRATIONS[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

        SolutionScanner.Scan(settings.ScanPath);

        var projectItems = SolutionScanner
                    .Solution!
                    .ProjectItems
                    .ToList();

        foreach (var projectItem in projectItems)
        {
            var projectName = projectItem.ProjectFile.Name.Replace(".csproj", "");

            foreach (var dbContextFactoryItem in projectItem.DbContextFactoryItems)
            {
                var dbContextName = dbContextFactoryItem.DbContextTypeName.Split('.').Last();

                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>($"Are you sure you want to [red]REMOVE A MIGRATION[/] from [blue]{dbContextName}[/]?")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(false)
                        .WithConverter(choice => choice ? "y" : "n"));

                if (!confirmation)
                {
                    AnsiConsole.MarkupLine($"[yellow]Skipping REMOVE MIGRATION from[/] [blue]{dbContextName}[/]");
                    continue;
                }

                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .StartAsync($"[green]Removing Migration from[/] [blue]{dbContextName}[/]", async ctx =>
                    {
                        try
                        {
                            await ProcessHelpers.RunProcessAsync(
                                "dotnet",
                                $"ef migrations remove --context {dbContextName} --verbose",
                                projectItem.ProjectFile.Directory!.FullName);

                            AnsiConsole.MarkupLine($"[green]Successfully removed last migration from[/] [blue]{dbContextName}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Failed to remove migration from[/] [blue]{dbContextName}[/]");
                            AnsiConsole.Write(ex.StackTrace!);
                        }
                    });
            }
        }

        return 0;
    }
}
