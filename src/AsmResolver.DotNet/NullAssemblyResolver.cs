namespace AsmResolver.DotNet;

/// <summary>
/// Provides an implementation of an assembly resolver that always resolves to <see cref="ResolutionStatus.AssemblyNotFound"/>.
/// </summary>
public sealed class NullAssemblyResolver : IAssemblyResolver
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullAssemblyResolver"/> class.
    /// </summary>
    public static NullAssemblyResolver Instance { get; } = new();

    /// <inheritdoc />
    public ResolutionStatus Resolve(AssemblyDescriptor assembly, ModuleDefinition? originModule, out AssemblyDefinition? result)
    {
        result = null;
        return ResolutionStatus.AssemblyNotFound;
    }
}
