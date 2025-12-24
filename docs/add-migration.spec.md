# Add Migrations Command Specification

The purpose of the `add-migrations` command is to create new migration files for database schema changes in an Entity Framework Core project. This specification outlines the requirements, options, and expected behavior of the `add-migrations` command.

## Assumptions

- The `add-migrations` command assumes that the user has a basic understanding of Entity Framework Core and that it can create migration code to facilitate database schema changes.
- The command will be run within the context of a .NET solution that has been properly configured to use Entity Framework Core.
- The command only works with DbContext factories that implement `IDesignTimeDbContextFactory<TContext>`.

## Features

### Add Migrations Command (AddMigrationsCommand.cs)

The `add-migrations` command supports two execution modes: **Interactive Mode** and **Command Line Mode**.

#### Mode Detection

The command automatically determines the execution mode based on the presence of command line options:
- **Interactive Mode**: When neither `--context` nor `--migrationName` options are provided.
- **Command Line Mode**: When either `--context` or `--migrationName` options are provided (both are required in this mode).

#### Interactive Mode

In interactive mode:
1. The command builds the solution first.
2. Scans for DbContexts with pending model changes by locating `IDesignTimeDbContextFactory<TContext>` implementations.
3. If no outstanding changes are detected, it displays a message and exits.
4. For each DbContext with pending changes:
   - Displays the list of pending model changes (marked as DESTRUCTIVE or NON-DESTRUCTIVE).
   - Prompts the user to confirm whether to create a migration.
   - If confirmed, prompts the user to enter the migration name.
   - Creates the migration using `dotnet ef migrations add`.
   - Rebuilds the solution after adding the migration.

#### Command Line Mode

In command line mode:
1. Both `--context` and `--migrationName` options are required.
2. The command validates that the specified DbContext exists in the solution.
3. Creates the migration directly without prompting for confirmation.
4. Rebuilds the solution after adding the migration.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |
| `--context <DbContextName>` | Name of the DbContext to use for the migration. Only a single context can be specified at a time. | Yes (Command Line Mode) |
| `--migrationName <name>` | Name of the migration to create. | Yes (Command Line Mode) |

### Settings Class (AddMigrationsSettings)

The `AddMigrationsSettings` class extends `NabsMigrationsSettings` and includes:

```csharp
public class AddMigrationsSettings : NabsMigrationsSettings
{
    [Description("Name of the DbContext to use for the migration. Required when using the command line.")]
    [CommandOption("--context")]
    public string? Context { get; init; }

    [Description("Name of the migration to create. Required when using the command line.")]
    [CommandOption("--migrationName")]
    public string? MigrationName { get; init; }

    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Context) && string.IsNullOrWhiteSpace(MigrationName);
}
```

### Migration Naming

- The migration name is prefixed with the DbContext name to ensure uniqueness across contexts.
- Format: `{DbContextName}-{MigrationName}` (e.g., `PrimaryDbContext-InitialCreate`).
- Should be provided in PascalCase (e.g., `AddCustomerTable`, `InitialCreate`).

### Output Directory

The output directory is determined by the tool and follows the pattern: `Migrations/{DbContextName}Migrations`.

For example:
- `PrimaryDbContext` → `Migrations/PrimaryDbContextMigrations/`
- `SecondaryDbContext` → `Migrations/SecondaryDbContextMigrations/`

## Usage Examples

### Interactive Mode

```powershell
# Run from current directory
nabs-migrations add-migrations

# Specify a solution path
nabs-migrations add-migrations ./MySolution
```

### Command Line Mode

```powershell
# Create migration for a specific DbContext
nabs-migrations add-migrations --context PrimaryDbContext --migrationName AddCustomerTable

# With a specific solution path
nabs-migrations add-migrations ./MySolution --context PrimaryDbContext --migrationName AddCustomerTable
```

## Error Handling

The command handles the following error scenarios:

| Scenario | Error Message |
|----------|---------------|
| `--context` provided without `--migrationName` | "Error: --migrationName is required when using command line mode." |
| `--migrationName` provided without `--context` | "Error: --context is required when using command line mode." |
| Specified DbContext not found | "Error: DbContext '{Context}' was not found in the solution." |
| Migration creation fails | "Failed to add migration to: {migrationName}" with stack trace |
| No outstanding changes (interactive) | "No outstanding changes detected in any DbContext. No migrations to add." |
