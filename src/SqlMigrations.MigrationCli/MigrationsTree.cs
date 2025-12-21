namespace SqlMigrations.MigrationCli;

internal static class MigrationsTree
{
    public static Tree Init(TreeOutputTypes treeOutputTypes = TreeOutputTypes.Migrations)
    {
        var root = new Tree($"{SolutionScanner.Solution!.SolutionFile.Name} Migrations")
            .Guide(TreeGuide.Line);

        foreach (var projectItem in SolutionScanner.Solution!.ProjectItems)
        {
            var projectNode = root
                .AddNode($"{projectItem.ProjectFile.Name.Replace(".csproj", "")}");

            foreach (var dbContextFactoryItem in projectItem.DbContextFactoryItems)
            {
                var dbContextNode = projectNode
                    .AddNode($"{dbContextFactoryItem.DbContextTypeName}");

                if (treeOutputTypes == TreeOutputTypes.Migrations
                     && dbContextFactoryItem.MigrationItems.Any())
                {
                    var migrationTable = new Table()
                        .AddColumns("Migration Name", "Status", "Created On")
                        .HideHeaders();

                    foreach (var migrationItem in dbContextFactoryItem.MigrationItems)
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

                    dbContextNode
                        .AddNode(migrationTable);
                }
                else if (treeOutputTypes == TreeOutputTypes.PendingModelChanges
                    && dbContextFactoryItem.PendingModelChanges.Any())
                {
                    var pendingModelChangeNode = dbContextNode
                        .AddNode("[red]Pending Model Changes[/]");
                    foreach (var pendingModelChange in dbContextFactoryItem.PendingModelChanges)
                    {
                        var statusColour = pendingModelChange.IsDestructive ? "red" : "yellow";
                        pendingModelChangeNode
                            .AddNode($"[{statusColour}]{pendingModelChange.Description}[/]");
                    }
                }
            }
        }

        return root;
    }
}
