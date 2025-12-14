var app = new CommandApp();

AnsiConsole.Clear();

var figlet = new FigletText("NABS Migrations")
    .Centered()
    .Color(Color.Blue);
AnsiConsole.Write(figlet);

app.Configure(config =>
{
    config.SetApplicationName("nabs-migrations");

    config.AddCommand<ScanCommand>("scan")
        .WithDescription("Scans a solution for EF Core migrations and displays them in a tree view.")
        .WithExample(["scan"])
        .WithExample(["scan", "./MySolution"]);

    config.AddCommand<MigrateCommand>("migrate")
        .WithDescription("Scans a solution for EF Core migrations and displays them in a tree view.")
        .WithExample(["migrate"])
        .WithExample(["migrate", "./MySolution"]);

    config.AddCommand<ResetCommand>("reset-db")
        .WithDescription("Resets the database. This is a destructive operation.")
        .WithExample(["reset-db"])
        .WithExample(["reset-db", "./MySolution"]);

    config.AddCommand<AddMigrationCommand>("add-migration")
        .WithDescription("Adds a new migration.")
        .WithExample(["add-migration"])
        .WithExample(["add-migration", "./MySolution"]);

    config.AddCommand<RemoveMigrationCommand>("remove-migration")
        .WithDescription("Remove a new migration.")
        .WithExample(["remove-migration"])
        .WithExample(["remove-migration", "./MySolution"]);
});

app.SetDefaultCommand<ScanCommand>();
//app.SetDefaultCommand<MigrateCommand>();
//app.SetDefaultCommand<DropDbCommand>();

await app.RunAsync(args);
