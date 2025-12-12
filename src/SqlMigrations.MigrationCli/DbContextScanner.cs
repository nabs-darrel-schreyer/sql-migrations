//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.DependencyInjection;

//namespace SqlMigrations.MigrationCli;

//public static class DbContextScanner
//{
//    private static List<DbContextItem> _items = [];

//    public static DirectoryInfo? SolutionFolder { get; private set; }
//    public static IReadOnlyList<DbContextItem> Items => _items.AsReadOnly();
    
//    public static DbContext? CreateDbContext(this DbContextItem item)
//    {
//        try
//        {
//            var factoryInstance = Activator.CreateInstance(item.DbContextFactoryType);
//            if (factoryInstance == null)
//            {
//                return null;
//            }

//            var createDbContextMethod = item.DbContextFactoryType.GetMethod("CreateDbContext");
//            if (createDbContextMethod == null)
//            {
//                return null;
//            }

//            var dbContext = createDbContextMethod.Invoke(factoryInstance, [Array.Empty<string>()]) as DbContext;
//            return dbContext;
//        }
//        catch
//        {
//            return null;
//        }
//    }

//    private static void ScanProject(DirectoryInfo projectDirectory)
//    {
//        if (projectDirectory == null)
//        {
//            return;
//        }

//        projectDirectory.ScanProject<DbContext>();

//        foreach (var dbContextType in projectDirectory.pro)
//        {
//            var factoryType = FindDesignTimeDbContextFactory(dbContextType.Type);
            
//            if (factoryType != null)
//            {
//                _items.Add(new DbContextItem
//                {
//                    AssemblyName = dbContextType.AssemblyName,
//                    DbContextTypeName = dbContextType.TypeName,
//                    DbContextType = dbContextType.Type,
//                    DbContextFactoryTypeName = factoryType.FullName ?? factoryType.Name,
//                    DbContextFactoryType = factoryType,
//                    ProjectPath = dbContextType.ProjectPath
//                });
//            }
//        }
//    }

//    private static Type? FindDesignTimeDbContextFactory(Type dbContextType)
//    {
//        try
//        {
//            var assembly = dbContextType.Assembly;
//            var factoryInterfaceType = typeof(IDesignTimeDbContextFactory<>).MakeGenericType(dbContextType);

//            return assembly.GetTypes()
//                .FirstOrDefault(t => t.IsClass 
//                    && !t.IsAbstract 
//                    && factoryInterfaceType.IsAssignableFrom(t));
//        }
//        catch
//        {
//            return null;
//        }
//    }
//}


