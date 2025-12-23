var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("nabs-migrations");

    var scanMigrationsItem = config.AddCommand<ListMigrationsCommand>("list-migrations")
        .WithDescription("Scans a solution for EF Core migrations and lists them in a tree view.")
        .WithExample(["list-migrations"])
        .WithExample(["list-migrations", "./MySolution"]);

    var scanPendingModelChangesItem = config.AddCommand<ListPendingModelChangesCommand>("list-pending-model-changes")
        .WithDescription("Scans a solution for EF Core pending model changes and list them in a tree view.")
        .WithExample(["list-pending-model-changes"])
        .WithExample(["list-pending-model-changes", "./MySolution"]);

    var migrateItem = config.AddCommand<ApplyMigrationsCommand>("apply-migrations")
        .WithDescription("Applies pending migrations to the local database.")
        .WithExample(["apply-migrations"])
        .WithExample(["apply-migrations", "./MySolution"])
        .WithExample(["apply-migrations", "--context", "PrimaryDbContext"])
        .WithExample(["apply-migrations", "--context", "PrimaryDbContext", "--migrationName", "InitialCreate"])
        .WithExample(["apply-migrations", "./MySolution", "--context", "PrimaryDbContext"]);

    var dropDbItem = config.AddCommand<DropDbCommand>("drop-db")
        .WithDescription("Drop the database. This is a destructive operation.")
        .WithExample(["drop-db"])
        .WithExample(["drop-db", "./MySolution"])
        .WithExample(["drop-db", "--context", "PrimaryDbContext"])
        .WithExample(["drop-db", "./MySolution", "--context", "PrimaryDbContext"]);

    var addMigrationItem = config.AddCommand<AddMigrationCommand>("add-migrations")
        .WithDescription("Adds new migrations.")
        .WithExample(["add-migrations"])
        .WithExample(["add-migrations", "./MySolution"])
        .WithExample(["add-migrations", "--context", "PrimaryDbContext", "--migrationName", "AddCustomerTable"])
        .WithExample(["add-migrations", "./MySolution", "--context", "PrimaryDbContext", "--migrationName", "AddCustomerTable"]);

    var removeMigrationItem = config.AddCommand<RemoveMigrationCommand>("remove-migration")
        .WithDescription("Remove the last migration from a DbContext.")
        .WithExample(["remove-migration"])
        .WithExample(["remove-migration", "./MySolution"])
        .WithExample(["remove-migration", "--context", "PrimaryDbContext"])
        .WithExample(["remove-migration", "./MySolution", "--context", "PrimaryDbContext"]);

    var resetMigrationsItem = config.AddCommand<ResetAllMigrationsCommand>("reset-all-migrations")
        .WithDescription("Delete all migrations and start over.")
        .WithExample(["reset-all-migrations"])
        .WithExample(["reset-all-migrations", "./MySolution"])
        .WithExample(["reset-all-migrations", "--project", "MyProject.DataMigrations"])
        .WithExample(["reset-all-migrations", "./MySolution", "--project", "MyProject.DataMigrations"]);

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
    { "List Migrations", "list-migrations" },
    { "List Pending Model Changes", "list-pending-model-changes" },
    { "Add Migration", "add-migrations" },
    { "Remove Migration", "remove-migrations" },
    { "Reset All Migrations", "reset-all-migrations" },
    { "Apply Migration", "apply-migrations" },
    { "Drop Database (only works for IDesignTimeDbContextFactory)", "drop-db" },
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
