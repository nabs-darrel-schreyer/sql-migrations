# Test Sequence

This document outlines the test sequence for the Nabs Migrations tool.

## Prerequisites

- Use the hard coded solution directory for testing purposes: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.

## Steps for Testing

1. **Setup the Environment**
   - Ensure that the solution at `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra` is accessible.
2. **Reset All Migrations**
   - Before starting the test, delete the existing migrations folder to ensure a clean migration process. The folder to delete is located at:
	 ```
	 C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra\src\AzdPipelinesAzureInfra.DataMigrations\Migrations
	 ```
	- OR
2. **Drop Existing Database**
	- Navigate to the solution directory in your terminal.
	- Run the following command to drop the existing database to ensure a clean state:
	```powershell
	nabs-migrations drop-db
	```
3. **Add Migrations**
	- Use the following commands to add migrations for both `PrimaryDbContext` and `SecondaryDbContext`:
	```powershell
	nabs-migrations add-migration --context PrimaryDbContext --migrationName InitialCreate
	nabs-migrations add-migration --context SecondaryDbContext --migrationName InitialCreate
	```
4. **Apply Migrations**
	- Apply the migrations to the database using the following commands:
	```powershell
	nabs-migrations apply-migration --context PrimaryDbContext
	nabs-migrations apply-migration --context SecondaryDbContext
	```
5. **Rebuild Solution**
	- After applying the migrations, rebuild the solution to ensure all changes are compiled:
	```powershell
	dotnet build C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra\AzdPipelinesAzureInfra.sln
	```
6. **Scan Migrations**
	- Finally, scan the migrations to verify their application:
	```powershell
	nabs-migrations scan-migrations
	```
	- Read the output to confirm that the migrations for the `PrimaryDbContext` and `SecondaryDbContext` have been applied successfully.

