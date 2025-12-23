namespace SqlMigrations.MigrationCli.Commands;

public class RemoveMigrationSettings : NabsMigrationsSettings
{
    [Description("Name of the DbContext to remove the migration from. Required when using the command line.")]
    [CommandOption("--context")]
    public string? Context { get; init; }

    /// <summary>
    /// Determines if the command should run in interactive mode.
    /// Interactive mode is used when the Context option is not provided.
    /// </summary>
    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Context);
}

internal sealed class RemoveMigrationCommand : AsyncCommand<RemoveMigrationSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, RemoveMigrationSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]REMOVE MIGRATIONS[/]");
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

            return await ExecuteCommandLineModeAsync(settings, cancellationToken);
        }

        return await ExecuteInteractiveModeAsync(settings, cancellationToken);
    }

    private async Task<int> ExecuteCommandLineModeAsync(RemoveMigrationSettings settings, CancellationToken cancellationToken)
    {
        await ProcessHelpers.BuildSolutionAsync(settings.ScanPath);

        SolutionScanner.Scan(settings.ScanPath);

        var projectItems = SolutionScanner
            .Solution!
            .ProjectItems
            .ToList();

        // Find the specified DbContext
        DbContextFactoryItem? targetDbContextItem = null;
        ProjectItem? targetProjectItem = null;

        foreach (var projectItem in projectItems)
        {
            foreach (var dbContextFactoryItem in projectItem.DbContextFactoryItems)
            {
                var contextName = dbContextFactoryItem.DbContextTypeName.Split('.').Last();
                if (contextName.Equals(settings.Context, StringComparison.OrdinalIgnoreCase))
                {
                    targetDbContextItem = dbContextFactoryItem;
                    targetProjectItem = projectItem;
                    break;
                }
            }
            if (targetDbContextItem != null) break;
        }

        if (targetDbContextItem == null || targetProjectItem == null)
        {
            AnsiConsole.MarkupLine($"[red]Error: DbContext '{settings.Context}' was not found in the solution.[/]");
            return 1;
        }

        var dbContextName = targetDbContextItem.DbContextTypeName.Split('.').Last();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"[green]Removing Migration from[/] [blue]{dbContextName}[/]", async ctx =>
            {
                try
                {
                    await ProcessHelpers.RunProcessAsync(
                        "dotnet",
                        $"ef migrations remove --context {dbContextName} --verbose",
                        targetProjectItem.ProjectFile.Directory!.FullName);

                    await ProcessHelpers.BuildSolutionAsync();

                    AnsiConsole.MarkupLine($"[green]Successfully removed last migration from[/] [blue]{dbContextName}[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to remove migration from[/] [blue]{dbContextName}[/]");
                    AnsiConsole.Write(ex.Message);
                    AnsiConsole.Write(ex.StackTrace ?? "No stack trace");
                }
            });

        return 0;
    }

    private async Task<int> ExecuteInteractiveModeAsync(RemoveMigrationSettings settings, CancellationToken cancellationToken)
    {
        await ProcessHelpers.BuildSolutionAsync(settings.ScanPath);

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

                            await ProcessHelpers.BuildSolutionAsync();

                            AnsiConsole.MarkupLine($"[green]Successfully removed last migration from[/] [blue]{dbContextName}[/]");
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Failed to remove migration from[/] [blue]{dbContextName}[/]");
                            AnsiConsole.Write(ex.Message);
                            AnsiConsole.Write(ex.StackTrace ?? "No stack trace");
                        }
                    });
            }
        }

        return 0;
    }
}
