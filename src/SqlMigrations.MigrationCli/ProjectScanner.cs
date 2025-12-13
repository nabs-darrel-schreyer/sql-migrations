namespace SqlMigrations.MigrationCli;

public static class ProjectScanner
{
    public static SolutionItem? Solution { get; private set; }

    public static void Scan(string? searchPath)
    {
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
    }

    public static void ScanForSolution(this DirectoryInfo solutionDirectory)
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

        Solution.ScanForProjects();
    }

    private static void ScanForProjects(this SolutionItem solutionItem)
    {
        var projectFiles = solutionItem.SolutionFile.Directory!
            .EnumerateFiles("*.csproj", SearchOption.AllDirectories);

        foreach (var projectFile in projectFiles)
        {
            var projectItem = new ProjectItem()
            {
                ProjectFile = projectFile
            };
            projectItem.ScanForDbContexts();

            if (projectItem.DbContextItems.Count > 0)
            {
                solutionItem.ProjectItems.Add(projectItem);
            }
        }
    }

    private static void ScanForDbContexts(this ProjectItem projectItem)
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

            dbContextItem.ScanForMigrations();

            projectItem.DbContextItems.Add(dbContextItem);
        }
    }

    private static void ScanForMigrations(this DbContextItem dbContextItem)
    {
        using var dbContext = dbContextItem.CreateDbContext()!;

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
                _ => outstandingChange.ToString() ?? "Unknown Operation"
            };
            
            var outstandingChangeItem = new OutstandingChangeItem
            {
                Description = description,
                IsDestructive = outstandingChange.IsDestructiveChange
            };
            dbContextItem.OutstandingChanges.Add(outstandingChangeItem);
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
