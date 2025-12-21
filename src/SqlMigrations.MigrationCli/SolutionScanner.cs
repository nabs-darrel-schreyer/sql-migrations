namespace SqlMigrations.MigrationCli;

public static class SolutionScanner
{
    private const string DesignTimeDbContextFactoryInterfaceName = "Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory`1";

    public static SolutionItem? Solution { get; private set; }

    private static readonly List<WeakReference<PluginLoadContext>> _loadContexts = [];

    public static void Scan(string? searchPath)
    {
        // Unload any previously loaded contexts before scanning again
        Unload();

        Solution = null;

        var solutionDirectory = string.IsNullOrWhiteSpace(searchPath)
            ? new DirectoryInfo(Directory.GetCurrentDirectory())
            : new DirectoryInfo(searchPath);

        while (solutionDirectory is not null)
        {
            ScanForSolution(solutionDirectory);

            if (Solution is not null)
            {
                break;
            }

            solutionDirectory = solutionDirectory.Parent;
        }

        // Unload assemblies after scan is complete - all Type references are no longer needed
        UnloadContexts();
    }

    public static void ScanForSolution(this DirectoryInfo solutionDirectory)
    {
        var solutionFile = solutionDirectory
            .EnumerateFiles()
            .FirstOrDefault(f => string.Equals(f.Extension, ".sln", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(f.Extension, ".slnx", StringComparison.OrdinalIgnoreCase));

        if (solutionFile == null)
        {
            while (solutionDirectory.Parent != null)
            {
                solutionDirectory = solutionDirectory.Parent;
                solutionFile = solutionDirectory
                    .EnumerateFiles()
                    .FirstOrDefault(f => string.Equals(f.Extension, ".sln", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(f.Extension, ".slnx", StringComparison.OrdinalIgnoreCase));
                if (solutionFile != null)
                {
                    break;
                }
            }
        }

        if (solutionFile == null)
        {
            return;
        }

        Solution = new SolutionItem
        {
            SolutionFile = solutionFile
        };

        Solution.ScanForDataMigrationProjects();
    }

    private static void ScanForDataMigrationProjects(this SolutionItem solutionItem)
    {
        var projectFiles = solutionItem.SolutionFile.Directory!
            .EnumerateFiles("*.csproj", SearchOption.AllDirectories);

        if (!projectFiles.Any())
        {
            return;
        }

        foreach (var projectFile in projectFiles)
        {
            if (!IsDataMigrationProject(projectFile))
            {
                continue;
            }

            var projectItem = new ProjectItem()
            {
                ProjectFile = projectFile
            };
            projectItem.ScanForDesignTimeDbContextFactory();

            if (projectItem.DbContextFactoryItems.Count > 0)
            {
                solutionItem.ProjectItems.Add(projectItem);
            }
        }
    }

    private static bool IsDataMigrationProject(FileInfo projectFile)
    {
        try
        {
            var projectContent = File.ReadAllText(projectFile.FullName);
            return projectContent.Contains("Nabs.Launchpad.Core.SeedData", StringComparison.OrdinalIgnoreCase)
                || projectContent.Contains("Nabs.Launchpad.Core.DataMigrations", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static void ScanForDesignTimeDbContextFactory(this ProjectItem projectItem)
    {
        var (projectAssembly, assemblyPath) = projectItem.ProjectFile.LoadProjectAssembly();
        if (projectAssembly == null || assemblyPath == null)
        {
            return;
        }

        var factoryTypes = projectAssembly.FindDesignTimeDbContextFactoryTypes().ToList();

        foreach (var factoryType in factoryTypes)
        {
            var factoryInterface = factoryType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.FullName?.StartsWith(DesignTimeDbContextFactoryInterfaceName) == true);

            if (factoryInterface == null)
            {
                continue;
            }

            var dbContextType = factoryInterface.GetGenericArguments()[0];

            var dbContextItem = new DbContextFactoryItem
            {
                AssemblyPath = assemblyPath,
                DbContextFactoryTypeName = factoryType.FullName ?? factoryType.Name,
                DbContextTypeName = dbContextType.FullName ?? dbContextType.Name,
                ProjectItem = projectItem,
            };

            ScanForMigrations(dbContextItem);

            projectItem.DbContextFactoryItems.Add(dbContextItem);
        }
    }

    private static void ScanForMigrations(DbContextFactoryItem dbContextItem)
    {
        var context = new PluginLoadContext(dbContextItem.AssemblyPath);
        _loadContexts.Add(new WeakReference<PluginLoadContext>(context));

        try
        {
            using var dbContext = CreateDbContextFromContext(context, dbContextItem);
            if (dbContext == null)
            {
                return;
            }

            var assemblyMigrations = dbContext.Database.GetMigrations();
            var appliedMigrations = dbContext.Database.GetAppliedMigrations();
            var pendingMigrations = dbContext.Database.GetPendingMigrations();

            foreach (var migration in assemblyMigrations)
            {
                var isApplied = appliedMigrations.Contains(migration);
                var isPending = pendingMigrations.Contains(migration);

                var migrationParts = migration.Split('_');
                var migrationName = migrationParts[1];
                var migrationCreatedOn = DateTime.ParseExact(migrationParts[0], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                var migrationItem = new MigrationItem
                {
                    FullName = migration,
                    Name = migrationName,
                    Status = isApplied ? "Applied" : isPending ? "Pending" : "Unknown",
                    CreatedOn = migrationCreatedOn,
                    AppliedOn = null
                };

                dbContextItem.MigrationItems.Add(migrationItem);
            }

            var outstandingChanges = dbContext.GetOutstandingModelChanges();
            foreach (MigrationOperation outstandingChange in outstandingChanges)
            {
                string description = outstandingChange switch
                {
                    AddColumnOperation addColumn => $"Add Column '{addColumn.Name}' to Table '{addColumn.Table}'",
                    DropColumnOperation dropColumn => $"Drop Column '{dropColumn.Name}' from Table '{dropColumn.Table}'",
                    AlterColumnOperation alterColumn => $"Alter Column '{alterColumn.Name}' in Table '{alterColumn.Table}'",
                    CreateTableOperation createTable => $"Create Table '{createTable.Name}'",
                    DropTableOperation dropTable => $"Drop Table '{dropTable.Name}'",
                    EnsureSchemaOperation ensureSchema => $"Ensure Schema '{ensureSchema.Name}'",
                    _ => outstandingChange.ToString() ?? "Unknown Operation"
                };

                var outstandingChangeItem = new PendingModelChangeItem
                {
                    Description = description,
                    IsDestructive = outstandingChange.IsDestructiveChange
                };
                dbContextItem.PendingModelChanges.Add(outstandingChangeItem);
            }
        }
        finally
        {
            context.Unload();
        }
    }

    private static DbContext? CreateDbContextFromContext(PluginLoadContext context, DbContextFactoryItem item)
    {
        try
        {
            var assembly = context.LoadFromAssemblyPathWithoutLock(item.AssemblyPath);

            var factoryType = assembly.GetType(item.DbContextFactoryTypeName);
            if (factoryType == null)
            {
                return null;
            }

            var factoryInstance = Activator.CreateInstance(factoryType);
            if (factoryInstance == null)
            {
                return null;
            }

            var createDbContextMethod = factoryType.GetMethod("CreateDbContext");
            if (createDbContextMethod == null)
            {
                return null;
            }

            return createDbContextMethod.Invoke(factoryInstance, [Array.Empty<string>()]) as DbContext;
        }
        catch
        {
            return null;
        }
    }

    private static (Assembly? Assembly, string? Path) LoadProjectAssembly(this FileInfo projectFile)
    {
        var projectDirectory = projectFile.Directory!;

        var binDebugDir = Path.Combine(projectDirectory.FullName, "bin", "Debug");
        var binReleaseDir = Path.Combine(projectDirectory.FullName, "bin", "Release");

        var result = TryLoadAssemblyFromDirectory(binDebugDir, projectFile.Name);

        if (result.Assembly == null)
        {
            result = TryLoadAssemblyFromDirectory(binReleaseDir, projectFile.Name);
        }

        return result;
    }

    private static IEnumerable<Type> FindDesignTimeDbContextFactoryTypes(this Assembly assembly)
    {
        try
        {
            var types = assembly.GetTypes();
            return types
                .Where(t => t != null && t.IsClass && !t.IsAbstract &&
                    t.GetInterfaces().Any(i => i.IsGenericType && i.FullName?.StartsWith(DesignTimeDbContextFactoryInterfaceName) == true));
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types
                .Where(t => t != null && t.IsClass && !t.IsAbstract &&
                    t.GetInterfaces().Any(i => i.IsGenericType && i.FullName?.StartsWith(DesignTimeDbContextFactoryInterfaceName) == true))!;
        }
    }

    private static (Assembly? Assembly, string? Path) TryLoadAssemblyFromDirectory(string directory, string projectFileName)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return (null, null);
            }

            var assemblyName = Path.GetFileNameWithoutExtension(projectFileName);
            var tfmDirs = Directory.GetDirectories(directory);

            foreach (var tfmDir in tfmDirs)
            {
                var dllPath = Path.Combine(tfmDir, $"{assemblyName}.dll");

                if (File.Exists(dllPath))
                {
                    var context = new PluginLoadContext(dllPath);
                    _loadContexts.Add(new WeakReference<PluginLoadContext>(context));
                    var result = context.LoadFromAssemblyPathWithoutLock(dllPath);
                    return (result, dllPath);
                }
            }

            return (null, null);
        }
        catch
        {
            return (null, null);
        }
    }

    /// <summary>
    /// Creates a DbContext instance from a DbContextFactoryItem by loading the assembly fresh.
    /// The caller is responsible for disposing the returned DbContext.
    /// Call <see cref="Unload"/> after you're done to release the assembly locks.
    /// </summary>
    public static DbContext? CreateDbContext(DbContextFactoryItem item)
    {
        try
        {
            var context = new PluginLoadContext(item.AssemblyPath);
            _loadContexts.Add(new WeakReference<PluginLoadContext>(context));
            var assembly = context.LoadFromAssemblyPathWithoutLock(item.AssemblyPath);

            var factoryType = assembly.GetType(item.DbContextFactoryTypeName);
            if (factoryType == null)
            {
                return null;
            }

            var factoryInstance = Activator.CreateInstance(factoryType);
            if (factoryInstance == null)
            {
                return null;
            }

            var createDbContextMethod = factoryType.GetMethod("CreateDbContext");
            if (createDbContextMethod == null)
            {
                return null;
            }

            return createDbContextMethod.Invoke(factoryInstance, [Array.Empty<string>()]) as DbContext;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Unloads all loaded plugin assemblies and releases file locks.
    /// Call this method when you're done using the scanned solution data.
    /// </summary>
    public static void Unload()
    {
        Solution = null;
        UnloadContexts();
    }

    private static void UnloadContexts()
    {
        foreach (var weakRef in _loadContexts)
        {
            if (weakRef.TryGetTarget(out var context))
            {
                context.Unload();
            }
        }

        _loadContexts.Clear();

        // Suggest garbage collection to release the assemblies
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
