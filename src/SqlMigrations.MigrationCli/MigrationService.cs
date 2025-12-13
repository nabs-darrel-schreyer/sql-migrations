namespace SqlMigrations.MigrationCli;

internal static class MigrationService
{
    public static IReadOnlyList<MigrationOperation> GetOutstandingModelChanges<TDbContext>(this TDbContext dbContext)
    where TDbContext : DbContext
    {
        var sourceSnapshot = dbContext.GetService<IMigrationsAssembly>().ModelSnapshot;
        var runtimeInitializer = dbContext.GetService<IModelRuntimeInitializer>();
        
        IRelationalModel? source = null;
        if (sourceSnapshot?.Model != null)
        {
            var initializedModel = runtimeInitializer.Initialize(sourceSnapshot.Model, designTime: true, validationLogger: null);
            source = initializedModel.GetRelationalModel();
        }

        var designTimeModel = dbContext.GetService<IDesignTimeModel>().Model;
        var target = designTimeModel.GetRelationalModel();

        var modelDiffer = dbContext.GetService<IMigrationsModelDiffer>();
        var changes = modelDiffer.GetDifferences(source, target);
        return changes;
    }

    public static async Task AddMigration<TDbContext>(this TDbContext context, string name)
        where TDbContext : DbContext
    {
        await RunMigration<TDbContext>(context, name, "add");
    }

    public static async Task RemoveMigration<TContext>(this TContext context, string name)
        where TContext : DbContext
    {
        await RunMigration<TContext>(context, "", "remove");
    }

    private static async Task RunMigration<TDbContext>(TDbContext context, string name, string command)
        where TDbContext : DbContext
    {
        var projectDirectory = "";

        if (projectDirectory == null)
        {
            Console.WriteLine($"Project directory for context type {typeof(TDbContext).FullName} not found.");
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"ef migrations {command} {name}",
            WorkingDirectory = projectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Error running {command} migration:\n{error}");
            return;
        }

        Console.WriteLine($"{name} migration {command} completed:\n" + output);
    }


}
