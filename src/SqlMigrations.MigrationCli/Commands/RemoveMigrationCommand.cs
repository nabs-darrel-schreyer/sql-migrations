namespace SqlMigrations.MigrationCli.Commands;

internal sealed class RemoveMigrationCommand : AsyncCommand<RemoveMigrationCommand.RemoveMigrationSettings>
{
    public sealed class RemoveMigrationSettings : CommandSettings
    {
        [Description("Path to scan. Defaults to current directory.")]
        [CommandArgument(0, "[searchPath]")]
        public string? ScanPath { get; init; }

        [Description("The name of the migration to remove.")]
        [CommandOption("-m|--migration-name")]
        public string? MigrationName { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, RemoveMigrationSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]REMOVE MIGRATIONS[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

        ProjectScanner.Scan(settings.ScanPath);

        var projectItems = ProjectScanner
                    .Solution!
                    .ProjectItems
                    .ToList();

        foreach (var projectItem in projectItems)
        {
            var projectName = projectItem.ProjectFile.Name.Replace(".csproj", "");

            var confirmation = AnsiConsole.Prompt(
                new TextPrompt<bool>($"Are you sure you want to [red]REMOVE A MIGRATION[/] to project: [blue]{projectName}[/]?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(false)
                    .WithConverter(choice => choice ? "y" : "n"));

            if (!confirmation)
            {
                AnsiConsole.MarkupLine($"[yellow]Skipping REMOVE MIGRATION for:[/] [blue]{projectName}[/]");
                continue;
            }


            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"[green]Removing Migration to:[/] [blue]{projectName}[/]", async ctx =>
                {
                    try
                    {
                        await projectItem.RunMigration(projectName, "remove");
                        AnsiConsole.MarkupLine($"[green]Successfully remove new migration from[/] [blue]{projectName}[/]");
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Failed to remove migration from[/] [blue]{projectName}[/]");
                        AnsiConsole.Write(ex.StackTrace!);
                    }
                });
        }

        return 0;
    }
}
