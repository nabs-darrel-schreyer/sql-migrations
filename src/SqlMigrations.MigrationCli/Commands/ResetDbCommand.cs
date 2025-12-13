using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace SqlMigrations.MigrationCli.Commands;

internal sealed class ResetDbCommand : Command<ResetDbCommand.ResetDbSettings>
{
    public sealed class ResetDbSettings : CommandSettings
    {
        [Description("Path to scan. Defaults to current directory.")]
        [CommandArgument(0, "[searchPath]")]
        public string? ScanPath { get; init; }
    }

    protected override int Execute(CommandContext context, ResetDbSettings settings, CancellationToken cancellationToken)
    {
        ProjectScanner.Scan(settings.ScanPath);

        var dbContextItems = ProjectScanner
                    .Solution!
                    .ProjectItems
                    .SelectMany(p => p.DbContextItems)
                    .ToList();

        foreach (var dbContextItem in dbContextItems)
        {
            using var dbContext = dbContextItem.CreateDbContext()!;
            var database = dbContext.Database.GetDbConnection().Database;

            AnsiConsole.MarkupLine($"[red]Deleting DB:[/] [blue]{database}[/]");
            dbContext.Database.EnsureDeleted();
            //AnsiConsole.MarkupLine($"[green]Creating DB:[/] [blue]{database}[/]");
            //dbContext.Database.EnsureCreated();
        }

        return 0;
    }
}
