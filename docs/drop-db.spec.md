# Drop DB Command Specification

The purpose of the `drop-db` command is to drop an existing database associated with a specified DbContext in an Entity Framework Core project. This specification outlines the requirements, options, and expected behavior of the `drop-db` command.

> **?? Warning**: This command is intended for **local database development only** and should **not be used in production scenarios**. It performs destructive operations that permanently delete databases.

## Assumptions

- The `drop-db` command assumes that the user has a basic understanding of Entity Framework Core and database management.
- The command will be run within the context of a .NET project that has been properly configured to use Entity Framework Core.
- The command only works with DbContext factories that implement `IDesignTimeDbContextFactory<TContext>`. This interface is used by Entity Framework Core design-time tools and ensures the command operates in a controlled development environment.

## Features

### Drop DB Command (DropDbCommand.cs)

The `drop-db` command currently operates in **Interactive Mode** only.

#### Interactive Mode

In interactive mode:
1. The command scans for available DbContexts in the solution by locating `IDesignTimeDbContextFactory<TContext>` implementations.
2. For each detected DbContext:
   - Creates an instance of the DbContext using the factory.
   - Retrieves the database name from the connection.
   - Prompts the user to confirm whether to drop the database.
   - If confirmed, drops the database using `DbContext.Database.EnsureDeleted()`.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |

### IDesignTimeDbContextFactory Requirement

The `drop-db` command discovers databases through `IDesignTimeDbContextFactory<TContext>` implementations. This is a design-time interface provided by Entity Framework Core that allows tools to create DbContext instances without requiring a running application.

Example factory implementation:

```csharp
public class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    const string connectionString = "Server=.;Database=SqlMigrationsDatabase;Integrated Security=True;TrustServerCertificate=True;";

    public TestDbContext CreateDbContext(string[] args)
    {
        var thisAssembly = typeof(TestDbContextFactory).Assembly;
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsAssembly(thisAssembly));
        return new TestDbContext(optionsBuilder.Options);
    }
}
```

## Usage Examples

### Interactive Mode

```powershell
# Run from current directory
nabs-migrations drop-db

# Specify a solution path
nabs-migrations drop-db ./MySolution
```

## Error Handling

The command handles the following scenarios:

| Scenario | Behavior |
|----------|----------|
| User declines confirmation | Displays "Skipping DB drop for: {databaseName}" and continues to next DbContext |
| Database does not exist | Displays "DB did not exist or could not be deleted: {databaseName}" |
| Database successfully deleted | Displays "Successfully deleted DB: {databaseName}" |

## Testing Process

- The `scanPath` is currently hard coded to the following solution directory for testing purposes: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.
- The project that contains the DbContext factories is: `AzdPipelinesAzureInfra.DataMigrations`.

### Test Commands

```powershell
# Test Interactive Mode
nabs-migrations drop-db