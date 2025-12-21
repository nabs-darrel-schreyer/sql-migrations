using System.Runtime.Loader;

namespace SqlMigrations.MigrationCli;

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _pluginDirectory;
    private static readonly HashSet<string> _sharedAssemblies =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.EntityFrameworkCore.Relational",
        "Microsoft.EntityFrameworkCore.Design",
        "Microsoft.EntityFrameworkCore.Abstractions"
    ];

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
        _pluginDirectory = Path.GetDirectoryName(pluginPath)!;
    }

    public Assembly LoadFromAssemblyPathWithoutLock(string assemblyPath)
    {
        using var stream = new MemoryStream(File.ReadAllBytes(assemblyPath));
        return LoadFromStream(stream);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Share EF Core assemblies with the host to avoid type identity issues
        if (assemblyName.Name != null && _sharedAssemblies.Contains(assemblyName.Name))
        {
            return null; // Fall back to default context
        }

        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            using var stream = new MemoryStream(File.ReadAllBytes(assemblyPath));
            return LoadFromStream(stream);
        }
        return null;
    }

    // Optional: for unmanaged DLLs
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }
        return IntPtr.Zero;
    }
}