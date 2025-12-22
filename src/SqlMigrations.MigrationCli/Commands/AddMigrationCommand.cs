namespace SqlMigrations.MigrationCli.Commands;

public class AddMigrationSettings : NabsMigrationsSettings
{
    [Description("Name of the DbContext to use for the migration. Required when using the command line.")]
    [CommandOption("--context")]
    public string? Context { get; init; }

    [Description("Name of the migration to create. Required when using the command line.")]
    [CommandOption("--migrationName")]
    public string? MigrationName { get; init; }

    /// <summary>
    /// Determines if the command should run in interactive mode.
    /// Interactive mode is used when neither Context nor MigrationName are provided.
    /// </summary>
    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Context) && string.IsNullOrWhiteSpace(MigrationName);
}

internal sealed class AddMigrationCommand : AsyncCommand<AddMigrationSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, AddMigrationSettings settings, CancellationToken cancellationToken)
    {
        var rule = new Rule("[yellow]ADD NEW MIGRATIONS[/]");
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
            if (string.IsNullOrWhiteSpace(settings.MigrationName))
            {
                AnsiConsole.MarkupLine("[red]Error: --migrationName is required when using command line mode.[/]");
                return 1;
            }

            return await ExecuteCommandLineModeAsync(settings, cancellationToken);
        }

        return await ExecuteInteractiveModeAsync(settings, cancellationToken);
    }

    private async Task<int> ExecuteCommandLineModeAsync(AddMigrationSettings settings, CancellationToken cancellationToken)
    {
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
            .StartAsync($"[green]Adding migration[/] [blue]{settings.MigrationName}[/] to [blue]{dbContextName}[/]", async ctx =>
            {
                try
                {
                    await ProcessHelpers.RunProcessAsync(
                        "dotnet",
                        $"ef migrations add {settings.MigrationName} --context {dbContextName} --output-dir Migrations/{dbContextName}Migrations --verbose",
                        targetProjectItem.ProjectFile.Directory!.FullName);

                    AnsiConsole.MarkupLine($"[green]Successfully added new migration:[/] [blue]{settings.MigrationName}[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to add migration:[/] [blue]{settings.MigrationName}[/]");
                    AnsiConsole.Markup(ex.StackTrace!);
                }
            });

        return 0;
    }

    private async Task<int> ExecuteInteractiveModeAsync(AddMigrationSettings settings, CancellationToken cancellationToken)
    {
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
                                $"ef migrations add {dbContextName}_{migrationName} --context {dbContextName} --output-dir Migrations/{dbContextName}Migrations --verbose",
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
