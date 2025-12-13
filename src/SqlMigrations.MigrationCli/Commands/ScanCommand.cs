namespace SqlMigrations.MigrationCli.Commands;

internal sealed class ScanCommand : Command<ScanCommand.ScanSettings>
{
    public sealed class ScanSettings : CommandSettings
    {
        [Description("Path to scan. Defaults to current directory.")]
        [CommandArgument(0, "[searchPath]")]
        public string? ScanPath { get; init; }
    }

    protected override int Execute(CommandContext context, ScanSettings settings, CancellationToken cancellationToken)
    {
        ProjectScanner.Scan(settings.ScanPath);

        var panel = new Panel(MigrationsTree.Init())
            .Header("[green]Solution scanned successfully![/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Green));

        AnsiConsole.Write(panel);




        return 0;
    }
}
