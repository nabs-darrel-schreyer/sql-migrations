# Add Migration Specification

The purpose of the `add-migration` command is to create new migration files for database schema changes in an Entity Framework Core project. This specification outlines the requirements, options, and expected behavior of the `add-migration` command.

## Assumptions

- The `add-migration` command assumes that the user has a basic understanding of Entity Framework Core and that it can create migration code to facilitate database schema changes.
- The command will be run within the context of a .NET project that has been properly configured to use Entity Framework Core.

## Features

### Add Migration Command (AddMigrationCommand.cs)

The `add-migration` command will support the following features:

- **Interactive Mode**: In interactive mode and before creating a migration, the user will be prompted to confirm each DbContext migration. The user will be prompted to enter the migration name.
- **Command Line Options**: The command will support the following options when run from the command line:
  - `--scanPath [<path>]`: Path to scan for the solution. Defaults to the current directory. The scanPath is optional when using the command line. In which case, the current directory will be used to locate the solution.
  - `--context <DbContextName>`: Name of the DbContext to use for the migration. When using the command line only a single context can be specified at a time.
  - `--migrationName <name>`: Name of the migration to create. Required when using the command line.
- **Migration Name**: Should be unique across all migration for the specified DbContext. The user will be prompted to provide a name for the migration. The migration name should be provided Hungarian Case. (e.g., `AddCustomerTable`).
- **Output Directory**: The output directory is determined by the tool and will be `Migrations/[DbContextName]Migrations`.

## Testing Process

- The `scanPath` is currently hard coded to the following solution directory for testing purposes: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.
- During this initial testing phase, remove the migrations folder to ensure a clean migration process. The folder to delete is located at: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra\src\AzdPipelinesAzureInfra.DataMigrations\Migrations`.
- The project that contains the migrations is: `AzdPipelinesAzureInfra.DataMigrations`. It contains two `DbContexts` and their associated Entities. The two `DbContext` are called:
  - `PrimaryDbContext`
  - `SecondaryDbContext`
 