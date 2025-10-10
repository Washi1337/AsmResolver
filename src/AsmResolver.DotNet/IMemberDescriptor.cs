namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a (reference to a) member defined in a .NET assembly.
    /// </summary>
    public interface IMemberDescriptor : IFullNameProvider, IModuleProvider, IImportable
    {
        /// <summary>
        /// When this member is defined in a type, gets the enclosing type.
        /// </summary>
        ITypeDescriptor? DeclaringType
        {
            get;
        }

        /// <summary>
        /// Imports the member using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported member.</returns>
        new IMemberDescriptor ImportWith(ReferenceImporter importer);

        /// <summary>
        /// Resolves the reference to a member definition, assuming the provided module as resolution context.
        /// </summary>
        /// <param name="context">The module to assume as resolution context.</param>
        /// <param name="definition">The resolved member definition, or <c>null</c> if the member could not be resolved.</param>
        /// <returns>A value describing the success or failure status of the member resolution.</returns>
        ResolutionStatus Resolve(RuntimeContext? context, out IMemberDefinition? definition);
    }
}
