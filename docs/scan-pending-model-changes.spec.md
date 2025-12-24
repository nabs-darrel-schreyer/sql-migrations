# List Pending Model Changes Command Specification

The purpose of the `list-pending-model-changes` command is to scan a solution for Entity Framework Core DbContexts and display any pending model changes that haven't been captured in migrations. This specification outlines the requirements, options, and expected behavior of the `list-pending-model-changes` command.

## Assumptions

- The `list-pending-model-changes` command assumes that the user has a basic understanding of Entity Framework Core and database migrations.
- The command will be run within the context of a .NET solution that has been properly configured to use Entity Framework Core.
- The command only works with DbContext factories that implement `IDesignTimeDbContextFactory<TContext>`.

## Features

### List Pending Model Changes Command (ListPendingModelChangesCommand.cs)

The `list-pending-model-changes` command operates in a single mode and displays all pending model changes found in the solution.

#### Behavior

1. The command builds the solution first.
2. Scans for all DbContexts in the solution by locating `IDesignTimeDbContextFactory<TContext>` implementations.
3. For each detected DbContext:
   - Compares the current model with the last migration's model snapshot.
   - Lists any pending changes that need to be captured in a new migration.
   - Indicates whether each change is destructive or non-destructive.
4. Displays the results in a tree view format inside a panel.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |

## Usage Examples

```powershell
# Run from current directory
nabs-migrations list-pending-model-changes

# Specify a solution path
nabs-migrations list-pending-model-changes ./MySolution
```

## Output Format

The command displays a panel with a tree view showing pending model changes for each DbContext. Changes are marked as:
- **NON-DESTRUCTIVE** (green): Safe changes like adding columns or tables
- **DESTRUCTIVE** (red): Potentially data-losing changes like dropping columns or tables

Example output:
```
???Solution scanned successfully!?????????????????????????????????????????
? SolutionName.sln Pending Model Changes                                 ?
? ??? ProjectName.DataMigrations                                         ?
?     ??? Namespace.PrimaryDbContext                                     ?
?     ?   ??? CreateTable: Customers (NON-DESTRUCTIVE)                   ?
?     ?   ??? AddColumn: Email on Orders (NON-DESTRUCTIVE)               ?
?     ??? Namespace.SecondaryDbContext                                   ?
?         ??? DropColumn: OldField on Products (DESTRUCTIVE)             ?
??????????????????????????????????????????????????????????????????????????
