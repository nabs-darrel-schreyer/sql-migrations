namespace SqlMigrations.MigrationCli.Commands;

internal sealed class BuildCommand : AsyncCommand<NabsMigrationsSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, NabsMigrationsSettings settings, CancellationToken cancellationToken)
    {
        SolutionScanner.Scan(settings.ScanPath);

        await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"[green]Building Solution:[/] [blue]{SolutionScanner.Solution!.SolutionFile.Name}[/]", async ctx =>
            {
                await ProcessHelpers.RunProcessAsync(
                    "dotnet",
                    "build",
                    SolutionScanner.Solution!.SolutionFile.Directory!.FullName);
            });

        AnsiConsole.MarkupLine("[green]Build finished![/]");

        return 0;
    }
}
