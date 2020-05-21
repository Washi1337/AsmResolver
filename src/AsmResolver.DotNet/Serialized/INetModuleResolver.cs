namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides members for resolving a reference to a net module. 
    /// </summary>
    public interface INetModuleResolver
    {
        /// <summary>
        /// Resolves a net module by its name.
        /// </summary>
        /// <param name="name">The name of the module to resolve.</param>
        /// <returns>The module, or <c>null</c> if the module could not be resolved.</returns>
        ModuleDefinition Resolve(string name);
    }
}