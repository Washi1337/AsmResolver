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

        /// <summary>
        /// Adds the assembly to the cache.
        /// </summary>
        /// <param name="descriptor">The reference to the assembly.</param>
        /// <param name="definition">The assembly.</param>
        void AddToCache(AssemblyDescriptor descriptor, AssemblyDefinition definition);

        /// <summary>
        /// Removees the assembly from the cache.
        /// </summary>
        /// <param name="descriptor">The reference to the assembly.</param>
        void RemoveFromCache(AssemblyDescriptor descriptor);

        /// <summary>
        /// Clears the cache.
        /// </summary>
        void ClearCache();
    }
}