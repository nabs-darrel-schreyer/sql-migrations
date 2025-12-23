namespace SqlMigrations.MigrationCli;

public static class AnsiConsoleHelpers
{
    extension(AnsiConsole)
    {
        public static async Task CreateSpinner(
            string promptMessage,
            string errorMessage,
            Func<Task> func)
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(promptMessage, async ctx =>
                {
                    try
                    {
                        await func();
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine(errorMessage);
                        AnsiConsole.Write(ex.Message);
                        AnsiConsole.Write(ex.StackTrace ?? "No stack trace");
                    }
                });
        }
    }
}
