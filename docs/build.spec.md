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
2. Executes `dotnet build` in the solution directory with a spinner indicating progress.
3. Displays "Build finished!" when the build completes.

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

## Output

The command displays:
1. A spinner with the message "Building Solution: {SolutionName}"
2. "Build finished!" upon successful completion
