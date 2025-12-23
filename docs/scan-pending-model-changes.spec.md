# Scan Pending Model Changes Command Specification

The purpose of the `scan-pending-model-changes` command is to scan a solution for Entity Framework Core DbContexts and display any pending model changes that haven't been captured in migrations. This specification outlines the requirements, options, and expected behavior of the `scan-pending-model-changes` command.

## Assumptions

- The `scan-pending-model-changes` command assumes that the user has a basic understanding of Entity Framework Core and database migrations.
- The command will be run within the context of a .NET project that has been properly configured to use Entity Framework Core.
- The command only works with DbContext factories that implement `IDesignTimeDbContextFactory<TContext>`.

## Features

### Scan Pending Model Changes Command (ScanPendingModelChangesCommand.cs)

The `scan-pending-model-changes` command operates in a single mode and displays all pending model changes found in the solution.

#### Behavior

1. The command scans for all DbContexts in the solution by locating `IDesignTimeDbContextFactory<TContext>` implementations.
2. For each detected DbContext:
   - Compares the current model with the last applied migration.
   - Lists any pending changes that need to be captured in a new migration.
   - Indicates whether each change is destructive or non-destructive.
3. Displays the results in a tree view format.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |

## Usage Examples

```powershell
# Run from current directory
nabs-migrations scan-pending-model-changes

# Specify a solution path
nabs-migrations scan-pending-model-changes ./MySolution
```

## Output Format

The command displays a tree view showing pending model changes for each DbContext. Changes are marked as:
- **NON-DESTRUCTIVE** (green): Safe changes like adding columns or tables
- **DESTRUCTIVE** (red): Potentially data-losing changes like dropping columns or tables

## Testing Process

- The `scanPath` is currently hard coded to the following solution directory for testing purposes: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.
- The project that contains the DbContext factories is: `AzdPipelinesAzureInfra.DataMigrations`.

### Test Commands

```powershell
# Test scan pending model changes
nabs-migrations scan-pending-model-changes
```
