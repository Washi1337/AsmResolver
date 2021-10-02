namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be referenced by a ResolutionScope coded index.
    /// </summary>
    public interface IResolutionScope : IMetadataMember, INameProvider, IModuleProvider
    {
        /// <summary>
        /// Gets the underlying assembly that this scope defines.
        /// </summary>
        AssemblyDescriptor? GetAssembly();
    }
}
