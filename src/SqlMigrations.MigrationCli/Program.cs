using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using Spectre.Console.Cli;

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
        .WithExample(["migrate", "./MySolution"]);

    config.AddCommand<MigrateCommand>("reset")
        .WithDescription("Resets the database. This is a destructive operation.")
        .WithExample(["reset", "./MySolution"]);
});

app.SetDefaultCommand<ScanCommand>();
//app.SetDefaultCommand<MigrateCommand>();
//app.SetDefaultCommand<ResetDbCommand>();

await app.RunAsync(args);
