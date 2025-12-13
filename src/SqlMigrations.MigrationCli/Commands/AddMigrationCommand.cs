namespace SqlMigrations.MigrationCli.Commands;

internal sealed class AddMigrationCommand : AsyncCommand<AddMigrationCommand.AddMigrationSettings>
{
    public sealed class AddMigrationSettings : CommandSettings
    {
        [Description("Path to scan. Defaults to current directory.")]
        [CommandArgument(0, "[searchPath]")]
        public string? ScanPath { get; init; }

        [Description("The name of the migration.")]
        [CommandOption("-m|--migration-name")]
        public string? MigrationName { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, AddMigrationSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]ADD NEW MIGRATIONS[/]");
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
                new TextPrompt<bool>($"Are you sure you want to [red]ADD A MIGRATION[/] to project: [blue]{projectName}[/]?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(false)
                    .WithConverter(choice => choice ? "y" : "n"));

            if (!confirmation)
            {
                AnsiConsole.MarkupLine($"[yellow]Skipping ADD MIGRATION for:[/] [blue]{projectName}[/]");
                continue;
            }

            
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"[green]Adding Migration to:[/] [blue]{projectName}[/]", async ctx =>
                {
                    try
                    {
                        await projectItem.RunMigration(projectName, "add");
                        AnsiConsole.MarkupLine($"[green]Successfully added new migration:[/] [blue]{projectName}[/]");
                    }
                    catch(Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Failed to add migration to:[/] [blue]{projectName}[/]");
                        AnsiConsole.Write(ex.StackTrace!);
                    }
                });
        }

        return 0;
    }
}
