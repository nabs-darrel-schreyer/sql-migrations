# Reset All Migrations Command Specification

The purpose of the `reset-all-migrations` command is to delete all migration files for a DataMigrations project. This specification outlines the requirements, options, and expected behavior of the `reset-all-migrations` command.

> **?? Warning**: This command is a **destructive operation** that permanently deletes all migration files. Use with caution and ensure you have backups if needed.

## Assumptions

- The `reset-all-migrations` command assumes that the user has a basic understanding of Entity Framework Core migrations and the structure of a DataMigrations project.
- The command will be run within the context of a .NET solution that has been properly configured to use Entity Framework Core.
- The command only works with projects that contain `IDesignTimeDbContextFactory<TContext>` implementations.
- The command assumes that the migrations are stored in a folder named `Migrations` within the DataMigrations project directory.
- The command assumes that the user has the necessary file system permissions to delete files in the migrations directory.

## Features

### Reset All Migrations Command (ResetAllMigrationsCommand.cs)

The `reset-all-migrations` command supports two execution modes: **Interactive Mode** and **Command Line Mode**.

#### Mode Detection

The command automatically determines the execution mode based on the presence of command line options:
- **Interactive Mode**: When the `--project` option is not provided.
- **Command Line Mode**: When the `--project` option is provided.

#### Interactive Mode

In interactive mode:
1. The command builds the solution first.
2. Scans for all data migrations projects in the solution that contain `IDesignTimeDbContextFactory<TContext>` implementations.
3. If no migration projects are found, displays a message and exits.
4. For each project:
   - Prompts the user to confirm whether to delete all migrations.
   - If confirmed, deletes the entire `Migrations` folder for that project.
   - Rebuilds the solution after deletion.

#### Command Line Mode

In command line mode:
1. The `--project` option is required.
2. The command scans for data migration projects in the solution.
3. Deletes all migrations for each found project directly without prompting for confirmation.
4. Rebuilds the solution after deletion.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |
| `--project <ProjectName>` | Name of the project to reset migrations for (without .csproj extension). | Yes (Command Line Mode) |

### Settings Class (ResetAllMigrationsSettings)

The `ResetAllMigrationsSettings` class extends `NabsMigrationsSettings` and includes:

```csharp
public class ResetAllMigrationsSettings : NabsMigrationsSettings
{
    [Description("Name of the project to reset migrations for. Required when using the command line.")]
    [CommandOption("--project")]
    public string? Project { get; init; }

    public bool IsInteractiveMode => string.IsNullOrWhiteSpace(Project);
}
```

## Usage Examples

### Interactive Mode

```powershell
# Run from current directory
nabs-migrations reset-all-migrations

# Specify a solution path
nabs-migrations reset-all-migrations ./MySolution
```

### Command Line Mode

```powershell
# Reset migrations for a specific project
nabs-migrations reset-all-migrations --project MyProject.DataMigrations

# With a specific solution path
nabs-migrations reset-all-migrations ./MySolution --project MyProject.DataMigrations
```

## Error Handling

The command handles the following scenarios:

| Scenario | Error Message / Behavior |
|----------|--------------------------|
| No data migration projects found (command line) | "No data migration projects were found in the solution." |
| No migration projects found (interactive) | "No migration projects found to reset." |
| No migrations folder exists | "No migrations folder found to delete for: {projectName}" |
| User declines confirmation (interactive) | "Skipping DELETE ALL MIGRATIONS for: {projectName}" |
| Deletion fails | "Failed to delete all migrations: {projectName}" with stack trace |
| Deletion succeeds | "Successfully deleted all migrations: {projectName}" |