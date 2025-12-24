using System.Text;

namespace SqlMigrations.MigrationCli;

internal static class ProcessHelpers
{
    /// <summary>
    /// Builds the solution. Can be called before or after SolutionScanner.Scan().
    /// </summary>
    /// <param name="scanPath">Optional path to scan for the solution. If null, uses current directory or SolutionScanner.Solution if available.</param>
    public static async Task BuildSolutionAsync(string? scanPath = null)
    {
        string solutionDirectory;

        if (SolutionScanner.Solution != null)
        {
            // Use already scanned solution
            solutionDirectory = SolutionScanner.Solution.SolutionFile.Directory!.FullName;
        }
        else
        {
            // Find solution directory from scanPath
            solutionDirectory = FindSolutionDirectory(scanPath);
        }

        await RunProcessAsync("dotnet", "build", solutionDirectory);
    }

    private static string FindSolutionDirectory(string? scanPath)
    {
        var directory = string.IsNullOrWhiteSpace(scanPath)
            ? new DirectoryInfo(Directory.GetCurrentDirectory())
            : new DirectoryInfo(scanPath);

        while (directory != null)
        {
            var solutionFile = directory
                .EnumerateFiles()
                .FirstOrDefault(f => string.Equals(f.Extension, ".sln", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(f.Extension, ".slnx", StringComparison.OrdinalIgnoreCase));

            if (solutionFile != null)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        // Fallback to the original scanPath or current directory
        return scanPath ?? Directory.GetCurrentDirectory();
    }

    public static async Task RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory)
    {
        // build the solution
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            //RedirectStandardOutput = true,
            //RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        //var output = new StringBuilder();
        //var error = new StringBuilder();

        //process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        //process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.Start();
        //process.BeginOutputReadLine();
        //process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        //AnsiConsole.MarkupLine("");
        //if (!string.IsNullOrWhiteSpace(output.ToString()))
        //{
        //    AnsiConsole.MarkupLine($"[grey]Output:[/] {Markup.Escape(output.ToString())}");
        //}
        //if (!string.IsNullOrWhiteSpace(error.ToString()))
        //{
        //    AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error.ToString())}");
        //}
        //AnsiConsole.MarkupLine("");
    }
}
