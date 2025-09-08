namespace AsmResolver.DotNet;

/// <summary>
/// Represents a named metadata object that introduces a new definition in a module.
/// </summary>
public interface IMetadataDefinition : IMetadataMember, INameProvider
{
    /// <summary>
    /// Gets the enclosing module that declares the object.
    /// </summary>
    ModuleDefinition? DeclaringModule
    {
        get;
    }
}
