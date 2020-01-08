namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type definition or reference that can be referenced by a TypeDefOrRef coded index. 
    /// </summary>
    public interface ITypeDefOrRef : ITypeDescriptor, IMemberRefParent, IHasCustomAttribute
    {
        /// <summary>
        /// When this type is nested, gets the enclosing type.
        /// </summary>
        new ITypeDefOrRef DeclaringType
        {
            get;
        }

        /// <summary>
        /// Resolves the reference to a type definition. 
        /// </summary>
        /// <returns>The resolved type definition, or <c>null</c> if the type could not be resolved.</returns>
        /// <remarks>
        /// This method can only be invoked if the reference was added to a module to 
        /// </remarks>
        TypeDefinition Resolve();
    }
}