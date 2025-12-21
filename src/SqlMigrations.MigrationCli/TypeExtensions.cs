using System.Text;

namespace SqlMigrations.MigrationCli;

internal static class TypeExtensions
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
}
