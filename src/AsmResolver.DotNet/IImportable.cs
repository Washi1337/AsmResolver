namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an entity in a .NET module that can be imported using the <see cref="ReferenceImporter"/>.
    /// </summary>
    public interface IImportable
    {
        /// <summary>
        /// Determines whether the descriptor of the member is fully imported in the provided module.
        /// </summary>
        /// <param name="module">The module that is supposed to import the member.</param>
        /// <returns><c>true</c> if the descriptor of the member is fully imported by the module, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This method verifies all references in the descriptor of the member, but does not verify the contents (e.g. a method body).
        /// </remarks>
        bool IsImportedInModule(ModuleDefinition module);
    }
}
