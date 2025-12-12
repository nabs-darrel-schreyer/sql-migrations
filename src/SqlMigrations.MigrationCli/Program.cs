using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorConsole.Core;
using SqlMigrations.MigrationCli;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureServices(services =>
{
    services.Scan();

    services.AddSingleton<MainViewModel>();
});

hostBuilder.UseRazorConsole<Main>();

await hostBuilder.RunConsoleAsync();

//    // Check if there are any migrations at all
//    var allMigrations = dbContext.Database.GetMigrations();
//    if (!allMigrations.Any())
//    {
//        Console.WriteLine("No migrations found in the assembly. Creating InitialCreate ...");
//        await dbContext.AddMigration("InitialCreate");
//        return;
//    }
//}

//await using (var serviceScope = serviceProvider.CreateAsyncScope())
//{
//    await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<TestDbContext>();

//    var hasPendingModelChanges = dbContext.Database.HasPendingModelChanges();
//    if (hasPendingModelChanges)
//    {
//        var pendingModelChanges = dbContext.GetPendingModelChanges();
//        foreach (var change in pendingModelChanges)
//        {
//            Console.WriteLine($"Found pending model change: {change}");
//        }
//    }
//}

//await using (var serviceScope = serviceProvider.CreateAsyncScope())
//{
//    await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<TestDbContext>();

//    var allMigrations = dbContext.Database.GetMigrations();
//    foreach (var migration in allMigrations)
//    {
//        Console.WriteLine($"Found migration: {migration}");
//    }

//    var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
//    foreach (var migration in appliedMigrations)
//    {
//        Console.WriteLine($"Applied migration: {migration}");
//    }



//    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
//    if (pendingMigrations.Any())
//    {
//        Console.WriteLine("Applying pending migrations...");
//        await dbContext.Database.MigrateAsync();
//        Console.WriteLine("Migrations applied successfully.");
//    }
//    else
//    {
//        Console.WriteLine("No pending migrations found.");
//    }

//}
