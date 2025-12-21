var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("nabs-migrations");

    var scanMigrationsItem = config.AddCommand<ScanMigrationsCommand>("scan-migrations")
        .WithDescription("Scans a solution for EF Core migrations and displays them in a tree view.")
        .WithExample(["scan-migrations"])
        .WithExample(["scan-migrations", "./MySolution"]);

    var scanPendingModelChangesItem = config.AddCommand<ScanPendingModelChangesCommand>("scan-pending-model-changes")
        .WithDescription("Scans a solution for EF Core pending model changes and displays them in a tree view.")
        .WithExample(["scan-pending-model-changes"])
        .WithExample(["scan-pending-model-changes", "./MySolution"]);

    var migrateItem = config.AddCommand<MigrateCommand>("migrate")
        .WithDescription("Scans a solution for EF Core migrations and displays them in a tree view.")
        .WithExample(["migrate"])
        .WithExample(["migrate", "./MySolution"]);

    var dropDbItem = config.AddCommand<DropDbCommand>("drop-db")
        .WithDescription("Drop the database. This is a destructive operation.")
        .WithExample(["drop-db"])
        .WithExample(["drop-db", "./MySolution"]);

    var addMigrationItem = config.AddCommand<AddMigrationCommand>("add-migration")
        .WithDescription("Adds a new migration.")
        .WithExample(["add-migration"])
        .WithExample(["add-migration", "./MySolution"]);

    var removeMigrationItem = config.AddCommand<RemoveMigrationCommand>("remove-migration")
        .WithDescription("Remove a new migration.")
        .WithExample(["remove-migration"])
        .WithExample(["remove-migration", "./MySolution"]);

    var resetMigrationsItem = config.AddCommand<ResetAllMigrationsCommand>("reset-all-migrations")
        .WithDescription("Delete all migrations and start over.")
        .WithExample(["reset-all-migrations"])
        .WithExample(["reset-all-migrations", "./MySolution"]);

    var buildItem = config.AddCommand<BuildCommand>("build")
        .WithDescription("Builds the solution.")
        .WithExample(["build"])
        .WithExample(["build", "./MySolution"]);
});

if(args.Length > 0)
{
    await app.RunAsync(args);
    return 0;
}

var menu = new Dictionary<string, string>()
{
    { "Scan Migrations", "scan-migrations" },
    { "Scan Pending Model Changes", "scan-pending-model-changes" },
    { "Add Migration", "add-migration" },
    { "Remove Migration", "remove-migration" },
    { "Reset All Migrations", "reset-all-migrations" },
    { "Migrate Database", "migrate" },
    { "Drop Database (at your own risk)", "drop-db" },
    { "Build Solution", "build" },
    { "Exit", "exit" }
};

while (true)
{
    AnsiConsole.Clear();

    var figlet = new FigletText("NABS Migrations")
        .Centered()
        .Color(Color.Blue);
    AnsiConsole.Write(figlet);

    var menuItem = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select an option:")
            .MoreChoicesText("[grey](Move up and down and press enter to select an item.)[/]")
            .AddChoices(menu.Keys));

    if (menuItem == "Exit")
    {
        AnsiConsole.MarkupLine("[green]Goodbye![/]");
        return 0;
    }

    if (!menu.TryGetValue(menuItem, out string? commandArgs))
    {
        commandArgs = "scan-migrations";
    }

    await app.RunAsync([ commandArgs ]);

    AnsiConsole.MarkupLine("\n[grey]Press any key to return to the menu...[/]");
    Console.ReadKey(true);
}
