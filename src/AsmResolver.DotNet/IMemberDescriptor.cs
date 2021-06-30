namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a (reference to a) member defined in a .NET assembly.
    /// </summary>
    public interface IMemberDescriptor : IFullNameProvider, IModuleProvider
    {
        /// <summary>
        /// When this member is defined in a type, gets the enclosing type.
        /// </summary>
        ITypeDescriptor? DeclaringType
        {
            get;
        }

        /// <summary>
        /// Resolves the reference to a member definition.
        /// </summary>
        /// <returns>The resolved member definition, or <c>null</c> if the member could not be resolved.</returns>
        /// <remarks>
        /// This method can only be invoked if the reference was added to a module.
        /// </remarks>
        IMemberDefinition? Resolve();
    }
}
