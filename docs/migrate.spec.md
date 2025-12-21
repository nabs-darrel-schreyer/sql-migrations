# Migrate Command Specification

The purpose of the `migrate` command is to facilitate the migration of data or configurations from one system or format to another. This specification outlines the requirements, options, and expected behavior of the `migrate` command.

## Assumptions

- The migrate command utilises the `SolutionScanner.Scan(settings.ScanPath);` method to identify projects and DbContexts within a solution.
- The command supports multiple projects and DbContexts.
- The command is designed to be user-friendly and provide clear feedback during execution.
- Each migration incorporates a human in the loop in order to confirm the migration steps before they are applied.

## Features

### Migrate Command (MigrateCommand.cs)

Currently the `migrate` command is partially implemented. It is only available using interactive mode. The following features need to be added:

When using the command line the following settings are available:
* `--scanPath [<path>]`: Path to scan for the solution. Defaults to current directory. The scanPath is optional when using the command line. In which case the current directory will be used to locate the solution.
* `--context <DbContextName>`: Name of the DbContext to migrate. When using the command line only a single context can be specified at a time. There is not option to migrate all contexts from the command line.
* `--migrationName <name>`: Name of the migration to apply. Required when using the command line.

When using the interactive mode the `--context` option is not available. Instead, the user will be prompted to confirm the DbContexts to migrate one at a time and provide the name of the migration.

Manual testing:
- I have hard coded the `scanPath` to the following solution directory: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.
- This CLI should find a project called: `AzdPipelinesAzureInfra.DataMigrations` that contains two `DbContexts` and their associated Entities.
- The two `DbContext` are called:
  - `PrimaryDbContext`
  - `SecondaryDbContext`
- During this initial testing phase, remove the migrations folders from 
- When running the `migrate` command in interactive mode, the user should be prompted to confirm each `DbContext` and provide a migration name for each.