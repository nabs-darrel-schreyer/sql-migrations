# Apply Migration Command Specification

The purpose of the `apply-migration` command is to apply pending Entity Framework Core migrations to the local database. This specification outlines the requirements, options, and expected behavior of the `apply-migration` command.

> **?? Warning**: This command is intended for **local database development only** and should **not be used in production scenarios**. For production deployments, use proper CI/CD pipelines with appropriate safeguards.

## Assumptions

- The `apply-migration` command assumes that the user has a basic understanding of Entity Framework Core and database migrations.
- The command will be run within the context of a .NET project that has been properly configured to use Entity Framework Core.
- The command only works with DbContext factories that implement `IDesignTimeDbContextFactory<TContext>`. This interface is used by Entity Framework Core design-time tools and ensures the command operates in a controlled development environment.
- The command utilizes the `SolutionScanner.Scan(settings.ScanPath)` method to identify projects and DbContexts within a solution.
- Each migration in interactive mode incorporates a human-in-the-loop confirmation before being applied.

## Features

### Apply Migration Command (ApplyMigrationCommand.cs)

The `apply-migration` command supports two execution modes: **Interactive Mode** and **Command Line Mode**.

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
   - Prompts the user to confirm whether to apply migrations to the database.
   - If confirmed, applies each pending migration using `DbContext.Database.Migrate(migrationName)`.

#### Command Line Mode

In command line mode:
1. The `--context` option is required.
2. The command validates that the specified DbContext exists in the solution.
3. If `--migrationName` is provided, only that specific migration is applied.
4. If `--migrationName` is not provided, all pending migrations are applied.
5. Applies the migration(s) directly without prompting for confirmation.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |
| `--context <DbContextName>` | Name of the DbContext to migrate. Only a single context can be specified at a time. | Yes (Command Line Mode) |
| `--migrationName <name>` | Name of the specific migration to apply. If not provided, all pending migrations will be applied. | No |

### Settings Class (ApplyMigrationSettings)

The `ApplyMigrationSettings` class extends `NabsMigrationsSettings` and includes:

```csharp
public class ApplyMigrationSettings : NabsMigrationsSettings
{
    [Description("Name of the DbContext to migrate. Required when using the command line.")]
    [CommandOption("--context")]
    public string? Context { get; init; }

    [Description("Name of the specific migration to apply. If not provided, all pending migrations will be applied.")]
    [CommandOption("--migrationName")]
    public string? MigrationName { get; init; }

    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Context);
}
```

### IDesignTimeDbContextFactory Requirement

The `apply-migration` command discovers databases through `IDesignTimeDbContextFactory<TContext>` implementations. This is a design-time interface provided by Entity Framework Core that allows tools to create DbContext instances without requiring a running application.

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
nabs-migrations apply-migration

# Specify a solution path
nabs-migrations apply-migration ./MySolution
```

### Command Line Mode

```powershell
# Apply all pending migrations for a specific DbContext
nabs-migrations apply-migration --context PrimaryDbContext

# Apply a specific migration for a DbContext
nabs-migrations apply-migration --context PrimaryDbContext --migrationName InitialCreate

# With a specific solution path
nabs-migrations apply-migration ./MySolution --context PrimaryDbContext
```

## Error Handling

The command handles the following scenarios:

| Scenario | Error Message / Behavior |
|----------|--------------------------|
| `--context` not provided in command line mode | "Error: --context is required when using command line mode." |
| Specified DbContext not found | "Error: DbContext '{Context}' was not found in the solution." |
| Specified migration not found | "Error: Migration '{MigrationName}' was not found in pending migrations." |
| No pending migrations | Displays "No pending migrations for database: {databaseName}" |
| User declines confirmation (interactive) | Displays "Skipping migration for: {databaseName}" and continues to next DbContext |
| Migration fails | Displays error message with stack trace |
| Migration succeeds | Displays "Successfully applied migration: {migrationName}" or "Successfully migrated database: {databaseName}" |

## Testing Process

- The `scanPath` is currently hard coded to the following solution directory for testing purposes: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.
- The project that contains the DbContext factories is: `AzdPipelinesAzureInfra.DataMigrations`. It contains two `DbContexts` and their associated Entities.
- The two `DbContext` are called:
  - `PrimaryDbContext`
  - `SecondaryDbContext`

### Test Commands

```powershell
# Test Interactive Mode
nabs-migrations apply-migration

# Test Command Line Mode - Apply all pending migrations
nabs-migrations apply-migration --context PrimaryDbContext

# Test Command Line Mode - Apply specific migration
nabs-migrations apply-migration --context PrimaryDbContext --migrationName InitialCreate

# Test Command Line Mode - SecondaryDbContext
nabs-migrations apply-migration --context SecondaryDbContext