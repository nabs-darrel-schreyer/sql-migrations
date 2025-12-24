# Test Sequence

This document outlines the test sequence for the NABS Migrations tool.

## Prerequisites

- Ensure you have a .NET solution with Entity Framework Core configured.
- The solution should contain at least one project with `IDesignTimeDbContextFactory<TContext>` implementations.

## Steps for Testing

### 1. Interactive Mode Testing

Run the tool without arguments to test the interactive menu:

```powershell
nabs-migrations
```

This should display the interactive menu with all available options.

### 2. List Pending Model Changes

Test scanning for pending model changes:

```powershell
# Interactive
nabs-migrations list-pending-model-changes

# With specific path
nabs-migrations list-pending-model-changes ./path/to/solution
```

### 3. List Migrations

Test listing existing migrations:

```powershell
# Interactive
nabs-migrations list-migrations

# With specific path
nabs-migrations list-migrations ./path/to/solution
```

### 4. Reset All Migrations (Clean Slate)

Before testing add/apply, optionally reset all migrations:

```powershell
# Interactive (prompts for confirmation)
nabs-migrations reset-all-migrations

# Command line
nabs-migrations reset-all-migrations --project MyProject.DataMigrations
```

### 5. Drop Existing Database

Drop any existing databases for a clean test:

```powershell
# Interactive (prompts for confirmation per database)
nabs-migrations drop-db

# Command line (specific context)
nabs-migrations drop-db --context PrimaryDbContext
```

### 6. Add Migrations

Test adding new migrations:

```powershell
# Interactive (walks through each DbContext with pending changes)
nabs-migrations add-migrations

# Command line
nabs-migrations add-migrations --context PrimaryDbContext --migrationName InitialCreate
nabs-migrations add-migrations --context SecondaryDbContext --migrationName InitialCreate
```

### 7. Apply Migrations

Test applying migrations to the database:

```powershell
# Interactive (prompts for each pending migration)
nabs-migrations apply-migrations

# Command line - all pending migrations
nabs-migrations apply-migrations --context PrimaryDbContext

# Command line - specific migration
nabs-migrations apply-migrations --context PrimaryDbContext --migrationName InitialCreate
```

### 8. Build Solution

Test the build command:

```powershell
nabs-migrations build
```

### 9. Verify Applied Migrations

After applying, verify the migrations are shown as "Applied":

```powershell
nabs-migrations list-migrations
```

### 10. Remove Migration

Test removing the last migration:

```powershell
# Interactive
nabs-migrations remove-migrations

# Command line
nabs-migrations remove-migrations --context PrimaryDbContext
```

## Full Test Cycle

For a complete test cycle, run these commands in order:

```powershell
# 1. Start fresh
nabs-migrations reset-all-migrations
nabs-migrations drop-db

# 2. Add migrations for all contexts
nabs-migrations add-migrations --context PrimaryDbContext --migrationName InitialCreate
nabs-migrations add-migrations --context SecondaryDbContext --migrationName InitialCreate

# 3. Verify migrations were created
nabs-migrations list-migrations

# 4. Apply migrations
nabs-migrations apply-migrations --context PrimaryDbContext
nabs-migrations apply-migrations --context SecondaryDbContext

# 5. Verify migrations are applied
nabs-migrations list-migrations

# 6. Test remove (optional)
nabs-migrations remove-migrations --context PrimaryDbContext

