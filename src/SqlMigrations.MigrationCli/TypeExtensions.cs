using Microsoft.EntityFrameworkCore;

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
}
