# NABS Migrations: A Developer-Friendly CLI for Entity Framework Core Migrations

## The Problem

I recently started a new project that involved significant experimentation with my SQL database schema. Like many .NET developers, I rely on `dotnet ef migrations` commands to manage database schema changes.

However, during rapid iteration, I found myself frequently needing to:
- Create new migrations for schema changes
- Apply migrations to the local database
- Roll back migrations that didn't work as expected
- Reset all migrations and even drop the entire database to start fresh

I started with PowerShell scripts, but quickly ran into issues:
- **Maintenance overhead**: Keeping scripts up-to-date as requirements evolved was tedious
- **Error-prone commands**: Typing long CLI commands in the terminal led to typos and mistakes
- **Context switching**: I kept forgetting the exact sequence of commands needed, especially after a few days away from the project

## The Solution: NABS Migrations

I decided to build a wrapper CLI tool that would streamline the migration management experience. The goal was to provide:

1. **Discoverability** - An interactive menu system so developers don't need to memorise commands
2. **Visibility** - Clear visualisation of pending model changes and migration status
3. **Safety** - Human-in-the-loop confirmation before destructive operations
4. **Flexibility** - Both interactive and command-line modes for different workflows
5. **Multi-context support** - Handle solutions with multiple DbContexts seamlessly

## Key Features

### ?? List Pending Model Changes

Before creating a migration, you want to know exactly what changes will be captured. The `list-pending-model-changes` command scans your solution and displays:
- All model changes that haven't been migrated yet
- Whether each change is **DESTRUCTIVE** (like dropping a column) or **NON-DESTRUCTIVE** (like adding a table)

This visibility helps developers make informed decisions about when and how to create migrations.

### ?? List Migrations

The `list-migrations` command provides a tree view of all migrations across your solution:
- Organised by project and DbContext
- Shows migration status (Applied/Pending)
- Includes timestamps for easy tracking

### ? Add Migrations

Creating migrations is simplified with the `add-migrations` command:
- **Interactive mode**: Walk through each DbContext with pending changes, see what will be migrated, and provide a meaningful name
- **Command-line mode**: Perfect for CI/CD pipelines with `--context` and `--migrationName` parameters
- Automatic output directory organisation per DbContext

### ?? Apply Migrations

The `apply-migrations` command applies pending migrations:
- **Interactive mode**: Review each pending migration and confirm before applying
- **Command-line mode**: Apply all pending migrations or target a specific one
- Displays a summary table of all pending migrations before execution

### ? Remove Migrations

Need to undo the last migration? The `remove-migrations` command handles it:
- Works with both interactive and command-line modes
- Automatically rebuilds the solution after removal

### ??? Reset All Migrations

Starting fresh? The `reset-all-migrations` command deletes all migration files:
- Scans for all migration projects in your solution
- Prompts for confirmation before deletion
- Useful when you need to consolidate migrations into a clean initial migration

### ?? Drop Database

For local development, sometimes you need a clean slate. The `drop-db` command:
- Discovers databases through your `IDesignTimeDbContextFactory` implementations
- Groups DbContexts that share the same database to avoid duplicate prompts
- Only works with design-time factories to ensure it's used in development environments

### ?? Build Solution

A convenience command to build your solution without leaving the tool.

## Interactive Menu

When you run `nabs-migrations` without any arguments, you get a beautiful interactive menu:

```
    ?????????????????????????????????????????
    ?         NABS Migrations               ?
    ?????????????????????????????????????????

    Select an option:
    > List Pending Model Changes
      List Migrations
      Add Migrations
      Remove Migration
      Reset All Migrations
      Apply Migration
      Drop Database
      Build Solution
      Exit
```

## Command-Line Mode for Automation

Every command also supports direct invocation with parameters, making it perfect for CI/CD pipelines:

```powershell
# Add a migration
nabs-migrations add-migrations --context PrimaryDbContext --migrationName AddCustomerTable

# Apply all pending migrations
nabs-migrations apply-migrations --context PrimaryDbContext

# Apply a specific migration
nabs-migrations apply-migrations --context PrimaryDbContext --migrationName AddCustomerTable
```

## Technical Design Decisions

### IDesignTimeDbContextFactory Requirement

The tool relies on `IDesignTimeDbContextFactory<TContext>` implementations to discover and work with your DbContexts. This design choice:
- Ensures the tool works at design-time without running your application
- Provides a clear contract for database configuration
- Follows the same pattern used by Entity Framework Core's own tooling

### Solution Scanning

The tool scans your entire solution to find:
- Projects containing EF Core migrations
- All `IDesignTimeDbContextFactory` implementations
- Existing migrations and their status
- Pending model changes

This enables a holistic view of migrations across multi-project solutions.

## Getting Started

1. Install the tool globally:
   ```powershell
   dotnet tool install --global nabs-migrations
   ```

2. Navigate to your solution directory and run:
   ```powershell
   nabs-migrations
   ```

3. Use the interactive menu or run specific commands directly.

## Conclusion

Managing Entity Framework Core migrations doesn't have to be a chore. With NABS Migrations, you get a streamlined, discoverable, and safe way to handle schema changes during development.

Whether you prefer an interactive workflow or need command-line automation for your CI/CD pipeline, the tool adapts to your needs while providing the visibility and safety guards that prevent common mistakes.

---

*Have you built any developer tools to improve your workflow? I'd love to hear about them in the comments!*

#dotnet #entityframework #efcore #developertools #cli #databasemigrations #softwareengineering