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
        /// Removes the assembly from the cache.
        /// </summary>
        /// <param name="descriptor">The reference to the assembly.</param>
        /// <returns>
        /// <c>true</c> if the assembly descriptor existed in the cache and was removed successfully,
        /// <c>false</c> otherwise.
        /// </returns>
        bool RemoveFromCache(AssemblyDescriptor descriptor);

        /// <summary>
        /// Determines whether the provided assembly descriptor was resolved before and stored in the cache.
        /// </summary>
        /// <param name="descriptor">The reference to the assembly.</param>
        /// <returns><c>true</c> if the assembly was resolved and cached, <c>false</c> otherwise.</returns>
        bool HasCached(AssemblyDescriptor descriptor);

        /// <summary>
        /// Clears the cache.
        /// </summary>
        void ClearCache();
    }
}