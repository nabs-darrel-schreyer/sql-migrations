namespace SqlMigrations.MigrationCli;

public sealed record SolutionItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public required FileInfo SolutionFile { get; init; }
    public List<ProjectItem> ProjectItems { get; } = [];
}

public sealed record ProjectItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public required FileInfo ProjectFile { get; init; }
    public List<DbContextItem> DbContextItems { get; } = [];
}

public sealed record DbContextItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public required Type DbContextType { get; init; }
    public required Type DbContextFactoryType { get; init; }
    public List<MigrationItem> MigrationItems { get; } = [];
}

public sealed record MigrationItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string FullName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedOn { get; init; }
    public DateTime? AppliedOn { get; set; }
}
