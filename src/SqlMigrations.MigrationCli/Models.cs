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
    public List<DbContextFactoryItem> DbContextFactoryItems { get; } = [];
}

public sealed record DbContextFactoryItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string AssemblyPath { get; init; }
    public required string DbContextFactoryTypeName { get; init; }
    public required string DbContextTypeName { get; init; }
    public required ProjectItem ProjectItem { get; init; }
    public List<MigrationItem> MigrationItems { get; } = [];
    public List<PendingModelChangeItem> PendingModelChanges { get; } = [];
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

public sealed record PendingModelChangeItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Description { get; init; } = string.Empty;
    public bool IsDestructive { get; init; }
}
