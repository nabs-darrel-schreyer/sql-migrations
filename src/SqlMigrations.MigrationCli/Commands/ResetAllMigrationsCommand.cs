namespace SqlMigrations.MigrationCli.Commands;

public class ResetAllMigrationsSettings : NabsMigrationsSettings
{
    [Description("Name of the project to reset migrations for. Required when using the command line.")]
    [CommandOption("--project")]
    public string? Project { get; init; }

    /// <summary>
    /// Determines if the command should run in interactive mode.
    /// Interactive mode is used when the Project option is not provided.
    /// </summary>
    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Project);
}

internal sealed class ResetAllMigrationsCommand : AsyncCommand<ResetAllMigrationsSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ResetAllMigrationsSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]RESET MIGRATIONS[/]");
        rule.LeftJustified();
        AnsiConsole.Write(rule);

        // Validate command line mode settings
        if (!settings.IsInteractiveMode)
        {
            if (string.IsNullOrWhiteSpace(settings.Project))
            {
                AnsiConsole.MarkupLine("[red]Error: --project is required when using command line mode.[/]");
                return 1;
            }

            return await ExecuteCommandLineModeAsync(settings, cancellationToken);
        }

        return await ExecuteInteractiveModeAsync(settings, cancellationToken);
    }

    private async Task<int> ExecuteCommandLineModeAsync(ResetAllMigrationsSettings settings, CancellationToken cancellationToken)
    {
        SolutionScanner.Scan(settings.ScanPath);

        var projectItems = SolutionScanner
            .Solution!
            .ProjectItems
            .ToList();

        if (projectItems.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No data migration projects were found in the solution.[/]");
            return 1;
        }

        foreach (var projectItem in projectItems)
        {
            var projectFileName = Path.GetFileNameWithoutExtension(projectItem.ProjectFile.FullName);

            await ExecuteAction(projectItem, projectFileName);
        }

        return 0;
    }

    private async Task<int> ExecuteInteractiveModeAsync(ResetAllMigrationsSettings settings, CancellationToken cancellationToken)
    {
        SolutionScanner.Scan(settings.ScanPath);

        var projectItems = SolutionScanner
                    .Solution!
                    .ProjectItems
                    .ToList();


        if (!projectItems.Any())
        {
            AnsiConsole.MarkupLine("[green]No migration projects found to reset.[/]");
            return 0;
        }

        foreach (var projectItem in projectItems)
        {
            var projectFileName = Path.GetFileNameWithoutExtension(projectItem.ProjectFile.FullName);

            var confirmation = AnsiConsole.Prompt(
                new TextPrompt<bool>($"Do you want to [red]DELETE ALL MIGRATIONS[/] for the project: [blue]{projectFileName}[/]?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(false)
                    .WithConverter(choice => choice ? "y" : "n"));

            if (!confirmation)
            {
                AnsiConsole.MarkupLine($"[yellow]Skipping DELETE ALL MIGRATIONS for:[/] [blue]{projectFileName}[/]");
                continue;
            }

            await ExecuteAction(projectItem, projectFileName);
        }

        return 0;
    }

    private async Task ExecuteAction(ProjectItem projectItem, string projectFileName)
    {
        await AnsiConsole.CreateSpinner(
                $"[green]Deleting all migrations for[/] [blue]{projectFileName}[/]",
                $"[red]Failed to delete all migrations:[/] [blue]{projectFileName}[/]",
                async () =>
                {
                    var migrationsFolder = Path.Combine(projectItem.ProjectFile.DirectoryName!, "Migrations");
                    if (Directory.Exists(migrationsFolder))
                    {
                        Directory.Delete(migrationsFolder, true);

                        await ProcessHelpers.BuildSolutionAsync();

                        AnsiConsole.MarkupLine($"[green]Successfully deleted all migrations:[/] [blue]{projectFileName}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[yellow]No migrations folder found to delete for:[/] [blue]{projectFileName}[/]");
                    }
                });
    }
}

