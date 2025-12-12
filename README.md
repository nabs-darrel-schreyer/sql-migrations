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

