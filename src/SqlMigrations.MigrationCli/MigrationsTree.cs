using Spectre.Console;

namespace SqlMigrations.MigrationCli;

internal static class MigrationsTree
{
    public static Tree Init()
    {
        var root = new Tree($"{ProjectScanner.Solution!.SolutionFile.Name} Migrations")
            .Guide(TreeGuide.Line);

        foreach (var projectItem in ProjectScanner.Solution!.ProjectItems)
        {
            var projectNode = root.AddNode($"{projectItem.ProjectFile.Name.Replace(".csproj", "")}");

            foreach (var dbContextItem in projectItem.DbContextItems)
            {
                var dbContextNode = projectNode.AddNode(dbContextItem.DbContextType.Name);

                var migrationTable = new Table()
                        .AddColumns("Migration Name", "Status", "Created On").HideHeaders();
                foreach (var migrationItem in dbContextItem.MigrationItems)
                {
                    var statusColour = migrationItem.Status switch
                    {
                        "Pending" => "yellow",
                        "Applied" => "green",
                        "Failed" => "red",
                        _ => "white"
                    };
                    migrationTable
                        .AddRow(
                            $"[{statusColour}]{migrationItem.Name}[/]", 
                            $"[{statusColour}]{migrationItem.Status}[/]", 
                            $"{migrationItem.CreatedOn}");

                }
                dbContextNode.AddNode(migrationTable);
            }
        }

        return root;
    }
}
