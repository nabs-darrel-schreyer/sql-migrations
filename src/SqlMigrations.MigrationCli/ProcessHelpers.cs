using System.Text;

namespace SqlMigrations.MigrationCli;

internal static class ProcessHelpers
{
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
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        AnsiConsole.MarkupLine("");
        if (!string.IsNullOrWhiteSpace(output.ToString()))
        {
            AnsiConsole.MarkupLine($"[grey]Output:[/] {Markup.Escape(output.ToString())}");
        }
        if (!string.IsNullOrWhiteSpace(error.ToString()))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error.ToString())}");
        }
        AnsiConsole.MarkupLine("");
    }
}
