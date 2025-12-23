# Drop DB Command Specification

The purpose of the `drop-db` command is to drop an existing database associated with a specified DbContext in an Entity Framework Core project. This specification outlines the requirements, options, and expected behavior of the `drop-db` command.

> **?? Warning**: This command is intended for **local database development only** and should **not be used in production scenarios**. It performs destructive operations that permanently delete databases.

## Assumptions

- The `drop-db` command assumes that the user has a basic understanding of Entity Framework Core and database management.
- The command will be run within the context of a .NET project that has been properly configured to use Entity Framework Core.
- The command only works with DbContext factories that implement `IDesignTimeDbContextFactory<TContext>`. This interface is used by Entity Framework Core design-time tools and ensures the command operates in a controlled development environment.

## Features

### Drop DB Command (DropDbCommand.cs)

The `drop-db` command supports two execution modes: **Interactive Mode** and **Command Line Mode**.

#### Mode Detection

The command automatically determines the execution mode based on the presence of command line options:
- **Interactive Mode**: When the `--context` option is not provided.
- **Command Line Mode**: When the `--context` option is provided.

#### Interactive Mode

In interactive mode:
1. The command scans for available DbContexts in the solution by locating `IDesignTimeDbContextFactory<TContext>` implementations.
2. For each detected DbContext:
   - Creates an instance of the DbContext using the factory.
   - Retrieves the database name from the connection.
   - Prompts the user to confirm whether to drop the database.
   - If confirmed, drops the database using `DbContext.Database.EnsureDeleted()`.

#### Command Line Mode

In command line mode:
1. The `--context` option is required.
2. The command validates that the specified DbContext exists in the solution.
3. Drops the database directly without prompting for confirmation.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |
| `--context <DbContextName>` | Name of the DbContext whose database should be dropped. | Yes (Command Line Mode) |

### Settings Class (DropDbSettings)

The `DropDbSettings` class extends `NabsMigrationsSettings` and includes:

```csharp
public class DropDbSettings : NabsMigrationsSettings
{
    [Description("Name of the DbContext whose database should be dropped. Required when using the command line.")]
    [CommandOption("--context")]
    public string? Context { get; init; }

    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Context);
}
```

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

### Command Line Mode

```powershell
# Drop database for a specific DbContext
nabs-migrations drop-db --context PrimaryDbContext

# With a specific solution path
nabs-migrations drop-db ./MySolution --context PrimaryDbContext
```

## Error Handling

The command handles the following scenarios:

| Scenario | Error Message / Behavior |
|----------|--------------------------|
| `--context` not found | "Error: DbContext '{Context}' was not found in the solution." |
| User declines confirmation (interactive) | "Skipping DB drop for: {databaseName}" |
| Database does not exist | "DB did not exist or could not be deleted: {databaseName}" |
| Database successfully deleted | "Successfully deleted DB: {databaseName}" |

## Testing Process

- The `scanPath` is currently hard coded to the following solution directory for testing purposes: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.
- The project that contains the DbContext factories is: `AzdPipelinesAzureInfra.DataMigrations`.
- The two `DbContext` are called:
  - `PrimaryDbContext`
  - `SecondaryDbContext`

### Test Commands

```powershell
# Test Interactive Mode
nabs-migrations drop-db

# Test Command Line Mode
nabs-migrations drop-db --context PrimaryDbContext