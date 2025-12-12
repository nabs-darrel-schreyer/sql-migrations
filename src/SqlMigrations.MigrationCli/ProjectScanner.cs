using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;

namespace SqlMigrations.MigrationCli;

public static class ProjectScanner
{
    public static SolutionItem? Solution { get; private set; }

    public static IServiceCollection Scan(this IServiceCollection services)
    {
        var solutionDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        
        while (solutionDirectory is not null)
        {
            ScanSolution(solutionDirectory);
            
            if (Solution is not null)
            {
                break;
            }
            
            solutionDirectory = solutionDirectory.Parent;
        }

        return services;
    }

    public static void ScanSolution(this DirectoryInfo solutionDirectory)
    {
        var solutionFile = solutionDirectory
            .EnumerateFiles()
            .FirstOrDefault(f => string.Equals(f.Extension, ".sln", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(f.Extension, ".slnx", StringComparison.OrdinalIgnoreCase));
        if (solutionFile == null)
        {
            return;
        }

        Solution = new SolutionItem
        {
            SolutionFile = solutionFile
        };

        Solution.ScanProjects();
    }

    private static void ScanProjects(this SolutionItem solutionItem)
    {
        var projectFiles = solutionItem.SolutionFile.Directory!
            .EnumerateFiles("*.csproj", SearchOption.AllDirectories);

        foreach (var projectFile in projectFiles)
        {
            var projectItem = new ProjectItem()
            {
                ProjectFile = projectFile
            };
            projectItem.ScanDbContexts();

            if (projectItem.DbContextItems.Count > 0)
            {
                solutionItem.ProjectItems.Add(projectItem);
            }
        }
    }

    private static void ScanDbContexts(this ProjectItem projectItem)
    {
        var projectAssembly = projectItem.ProjectFile.LoadProjectAssembly();
        if (projectAssembly == null)
        {
            return;
        }

        var dbContextTypes = projectAssembly.FindTypesAssignableTo<DbContext>().ToList();

        foreach (var dbContextType in dbContextTypes)
        {
            // Find corresponding factory type (assuming naming convention: {DbContextName}Factory)
            var factoryTypeName = $"{dbContextType.Name}Factory";
            var factoryType = projectAssembly.GetTypes()
                .FirstOrDefault(t => t.Name == factoryTypeName && 
                                    typeof(IDesignTimeDbContextFactory<>).MakeGenericType(dbContextType).IsAssignableFrom(t));
            
            if (factoryType == null)
            {
                continue;
            }

            var dbContextItem = new DbContextItem
            {
                DbContextType = dbContextType,
                DbContextFactoryType = factoryType
            };

            dbContextItem.ScanMigrations();

            projectItem.DbContextItems.Add(dbContextItem);
        }
    }

    private static void ScanMigrations(this DbContextItem dbContextItem)
    {
        using var dbContext = dbContextItem.CreateDbContext();

        var migrations = dbContext.Database.GetMigrations();

        foreach (var migration in migrations)
        {
            var migrationParts = migration.Split('_');
            var migrationItem = new MigrationItem
            {
                FullName = migration,
                Name = migrationParts[1],
                AppliedOn = DateTime.ParseExact(migrationParts[0], "yyyyMMddHHmmss", CultureInfo.InvariantCulture)
            };
            dbContextItem.Migrations.Add(migrationItem);
        }

    }

    private static Assembly? LoadProjectAssembly(this FileInfo projectFile)
    {
        var projectDirectory = projectFile.Directory;
        if (projectDirectory == null)
        {
            return null;
        }

        var binDebugDir = Path.Combine(projectDirectory.FullName, "bin", "Debug");
        var binReleaseDir = Path.Combine(projectDirectory.FullName, "bin", "Release");

        var assembly = TryLoadAssemblyFromDirectory(binDebugDir, projectFile.Name);

        assembly ??= TryLoadAssemblyFromDirectory(binReleaseDir, projectFile.Name);

        return assembly;
    }

    private static IEnumerable<Type> FindTypesAssignableTo<TTypeToFind>(this Assembly assembly)
    {
        try
        {
            var types = assembly.GetTypes();
            return types
                .Where(t => t.IsClass && !t.IsAbstract && typeof(TTypeToFind).IsAssignableFrom(t));
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types
                .Where(t => t != null && t.IsClass && !t.IsAbstract && typeof(TTypeToFind).IsAssignableFrom(t))!;
        }
    }

    private static Assembly? TryLoadAssemblyFromDirectory(string directory, string projectFileName)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }

            var assemblyName = Path.GetFileNameWithoutExtension(projectFileName);
            var tfmDirs = Directory.GetDirectories(directory);

            foreach (var tfmDir in tfmDirs)
            {
                var dllPath = Path.Combine(tfmDir, $"{assemblyName}.dll");

                if (File.Exists(dllPath))
                {
                    return Assembly.LoadFrom(dllPath);
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
