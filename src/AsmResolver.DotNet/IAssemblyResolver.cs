namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for resolving references to external .NET assemblies.
    /// </summary>
    public interface IAssemblyResolver
    {
        /// <summary>
        /// Resolves a reference to an assembly. 
        /// </summary>
        /// <param name="assembly">The reference to the assembly.</param>
        /// <returns>The resolved assembly, or <c>null</c> if the resolution failed.</returns>
        AssemblyDefinition Resolve(AssemblyDescriptor assembly);

        void AddToCache(AssemblyDescriptor assembly, AssemblyDefinition definition);

        void RemoveFromCache(AssemblyDescriptor assembly);

        void ClearCache();
    }
}