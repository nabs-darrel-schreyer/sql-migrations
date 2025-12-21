# sql-migrations

Demonstrate how to do SQL migrations.

## Prerequisites

```powershell
dotnet tools install --global dotnet-ef
```

Make sure it is installed by running:

```powershell
dotnet ef
```

You should see the help output for the `dotnet ef` command.

## Bundle the Migrations

```powershell
dotnet ef migrations bundle --project <YourProjectName> --output ./migrations-bundle
```

This command will create a self-contained executable in the `./migrations-bundle` directory.

You can then run the migrations using the following command:

```powershell
dotnet ./migrations-bundle/YourProjectName.dll
```

As a convenience, there is a script included in the `scripts` folder of the solution.
You can run it like this:
```powershell
.\scripts\migrations.ps1
```

This script will create the bundle and then run it.

# NabsMigrations Tool

Using the Entity Framework Core CLI can be cumbersome. You have to remember a lot of commands and options.

The NabsMigrations tool aims to simplify this process by providing a more intuitive interface for managing your migrations.

You can find the NabsMigrations tool here: [NabsMigrations GitHub Repository](https://github.com/Nabs/NabsMigrations)

## Goals

- Provide a statistical summary of migrations in a solution.
- Visualise all pending model changes in a solution.
- Visualise all migrations in a solution.
- Simplify the process of managing SQL migrations.
- Provide developers with the ability to understand and control migrations easily.
- Offer a user-friendly command-line interface for common migration tasks.
- Support multiple projects within a solution.
- Support multiple DbContexts within a project.
- Enable easy rollback of migrations.
- Provide easy capability to drop local databases and migrate them from scratch.
- Facilitate automated migration deployment in CI/CD pipelines.

## Features

### Statistical Summary of Migrations

The tool can provide a statistical summary of migrations in your solution, including the number of migrations per project and DbContext.

### Visualise All Pending Model Changes

The tool can scan your solution for pending model changes and display them in a hierarchy.

### Adding Migrations

You can add a new migration to a specific project with guided step-by-step prompts.

### Applying Migrations

You can apply pending migrations to your database with a single command.

### Reset Migrations

You can delete all migrations and start over again.

### Rollback Migrations

You can rollback the last applied migration if needed.

