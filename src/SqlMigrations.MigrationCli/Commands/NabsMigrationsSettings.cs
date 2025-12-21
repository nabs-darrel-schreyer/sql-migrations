namespace SqlMigrations.MigrationCli.Commands;

public class NabsMigrationsSettings : CommandSettings
{
    [Description("Path to scan. Defaults to current directory.")]
    [CommandArgument(0, "[searchPath]")]
    public string? ScanPath { get; init; } = @"C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra\";
}
