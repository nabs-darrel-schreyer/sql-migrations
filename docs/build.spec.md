# Build Command Specification

The purpose of the `build` command is to build the solution that contains the Entity Framework Core migrations projects. This specification outlines the requirements, options, and expected behavior of the `build` command.

## Assumptions

- The `build` command assumes that the user has .NET SDK installed and available in the system PATH.
- The command will be run within the context of a .NET solution that can be built using `dotnet build`.

## Features

### Build Command (BuildCommand.cs)

The `build` command operates in a single mode and builds the entire solution.

#### Behavior

1. The command scans for the solution file at the specified path.
2. Executes `dotnet build` in the solution directory.
3. Displays the build progress with a spinner.
4. Reports when the build is finished.

### Command Line Options

The command supports the following options:

| Option | Description | Required |
|--------|-------------|----------|
| `[scanPath]` | Path to scan for the solution. Defaults to the current directory. Provided as a positional argument. | No |

## Usage Examples

```powershell
# Run from current directory
nabs-migrations build

# Specify a solution path
nabs-migrations build ./MySolution
```

## Testing Process

- The `scanPath` is currently hard coded to the following solution directory for testing purposes: `C:\Dev\nabs-darrel-schreyer\azd-pipelines-azure-infra`.

### Test Commands

```powershell
# Test build
nabs-migrations build
```
