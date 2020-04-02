namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a type in a managed assembly. 
    /// </summary>
    public interface ITypeDescriptor : IMemberDescriptor
    {
        /// <summary>
        /// Gets the namespace the type resides in.
        /// </summary>
        string Namespace
        {
            get;
        }

        /// <summary>
        /// Gets the resolution scope that defines the type.
        /// </summary>
        IResolutionScope Scope
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether instances of this type are passed on by value or by reference.
        /// </summary>
        bool IsValueType
        {
            get;
        }
        
        /// <summary>
        /// Resolves the reference to a type definition. 
        /// </summary>
        /// <returns>The resolved type definition, or <c>null</c> if the type could not be resolved.</returns>
        /// <remarks>
        /// This method can only be invoked if the reference was added to a module. 
        /// </remarks>
        TypeDefinition Resolve();

        /// <summary>
        /// Calculates the runtime size of the type.
        /// </summary>
        /// <param name="is32Bit"></param>
        /// <returns>The size of the type, in bytes</returns>
        int CalculateSize(bool is32Bit);
    }
}