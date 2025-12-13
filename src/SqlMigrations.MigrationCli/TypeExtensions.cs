namespace SqlMigrations.MigrationCli;

internal static class TypeExtensions
{
    public static DbContext? CreateDbContext(this DbContextItem item)
    {
        try
        {
            var factoryInstance = Activator.CreateInstance(item.DbContextFactoryType);
            if (factoryInstance == null)
            {
                return null;
            }

            var createDbContextMethod = item.DbContextFactoryType.GetMethod("CreateDbContext");
            if (createDbContextMethod == null)
            {
                return null;
            }

            var dbContext = createDbContextMethod.Invoke(factoryInstance, [Array.Empty<string>()]) as DbContext;
            return dbContext;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> RunMigration(this ProjectItem projectItem, string name, string command)
    {
        var projectDirectory = projectItem.ProjectFile.Directory!;

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"ef migrations {command} {name}",
            WorkingDirectory = projectDirectory.FullName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }
}
