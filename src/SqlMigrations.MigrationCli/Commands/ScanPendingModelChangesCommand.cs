namespace SqlMigrations.MigrationCli.Commands;

internal sealed class ScanPendingModelChangesCommand : AsyncCommand<NabsMigrationsSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, NabsMigrationsSettings settings, CancellationToken cancellationToken)
    {
        SolutionScanner.Scan(settings.ScanPath);

        var panel = new Panel(MigrationsTree.Init(TreeOutputTypes.PendingModelChanges))
            .Header("[green]Solution scanned successfully![/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Green));

        AnsiConsole.Write(panel);

        return 0;
    }
}
