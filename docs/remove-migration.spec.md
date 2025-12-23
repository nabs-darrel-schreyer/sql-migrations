# Remove Migration Command Specification

The purpose of the `remove-migration` command is to remove the last migration for a specified DbContext in an Entity Framework Core project. This specification outlines the requirements, options, and expected behavior of the `remove-migration` command.

## Assumptions

- The `remove-migration` command assumes that the user has a basic understanding of Entity Framework Core and database migrations.
- The command will be run within the context of a .NET project that has been properly configured to use Entity Framework Core.
- The command removes the last unapplied migration. If the migration has been applied to the database, it must be reverted first.

## Features

### Remove Migration Command (RemoveMigrationCommand.cs)

The `remove-migration` command supports two execution modes: **Interactive Mode** and **Command Line Mode**.

#### Mode Detection

The command automatically determines the execution mode based on the presence of command line options:
- **Interactive Mode**: When the `--context` option is not provided.
- **Command Line Mode**: When the `--context` option is provided.

#### Interactive Mode

In interactive mode:
1. The command scans for all DbContexts in the solution.
2. For each detected DbContext:
   - Prompts the user to confirm whether to remove the last migration.
   - If confirmed, removes the last migration using `dotnet ef migrations remove`.

#### Command Line Mode

In command line mode:
1. The `--context` option is required.
2. The command validates that the specified DbContext exists in the solution.
3. Removes the last migration directly without prompting for confirmation.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |
| `--context <DbContextName>` | Name of the DbContext to remove the migration from. | Yes (Command Line Mode) |

### Settings Class (RemoveMigrationSettings)

The `RemoveMigrationSettings` class extends `NabsMigrationsSettings` and includes:

```csharp
public class RemoveMigrationSettings : NabsMigrationsSettings
{
    [Description("Name of the DbContext to remove the migration from. Required when using the command line.")]
    [CommandOption("--context")]
    public string? Context { get; init; }

    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Context);
}
```

## Usage Examples

### Interactive Mode

```powershell
# Run from current directory
nabs-migrations remove-migration

# Specify a solution path
nabs-migrations remove-migration ./MySolution
```

### Command Line Mode

```powershell
# Remove last migration for a specific DbContext
nabs-migrations remove-migration --context PrimaryDbContext

# With a specific solution path
nabs-migrations remove-migration ./MySolution --context PrimaryDbContext
```

## Error Handling

The command handles the following scenarios:

| Scenario | Error Message / Behavior |
|----------|--------------------------|
| `--context` not found | "Error: DbContext '{Context}' was not found in the solution." |
| User declines confirmation (interactive) | "Skipping REMOVE MIGRATION from {dbContextName}" |
| Removal fails | "Failed to remove migration from {dbContextName}" with stack trace |
| Removal succeeds | "Successfully removed last migration from {dbContextName}" |

## Testing Process

- The `scanPath` is currently hard coded to the following solution directory for testing purposes: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.
- The project that contains the DbContext factories is: `AzdPipelinesAzureInfra.DataMigrations`.
- The two `DbContext` are called:
  - `PrimaryDbContext`
  - `SecondaryDbContext`

### Test Commands

```powershell
# Test Interactive Mode
nabs-migrations remove-migration

# Test Command Line Mode
nabs-migrations remove-migration --context PrimaryDbContext
```
