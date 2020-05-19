namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an accessible member defined in a metadata image.
    /// </summary>
    public interface IMemberDefinition : IMemberDescriptor, IMetadataMember
    {
        /// <summary>
        /// Gets the type that declares the member (if available).
        /// </summary>
        new TypeDefinition DeclaringType
        {
            get;
        }
        
        /// <summary>
        /// Determines whether the member can be accessed from the scope that is determined by the provided type.
        /// </summary>
        /// <param name="type">The type defining the scope.</param>
        /// <returns>True if the scope of the provided type can access the member, false otherwise.</returns>
        bool IsAccessibleFromType(TypeDefinition type);
    }
}