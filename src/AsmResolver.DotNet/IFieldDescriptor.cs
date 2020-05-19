using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a field in a managed assembly. 
    /// </summary>
    public interface IFieldDescriptor : IMemberDescriptor, IMetadataMember
    {
        /// <summary>
        /// Gets the signature of the field.
        /// </summary>
        FieldSignature Signature
        {
            get;
        }

        /// <summary>
        /// Resolves the reference to a field definition. 
        /// </summary>
        /// <returns>The resolved field definition, or <c>null</c> if the field could not be resolved.</returns>
        /// <remarks>
        /// This method can only be invoked if the reference was added to a module. 
        /// </remarks>
        FieldDefinition Resolve();
    }
}