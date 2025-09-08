using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a field in a managed assembly.
    /// </summary>
    public interface IFieldDescriptor : IMemberDescriptor, IMetadataMember
    {
        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        new Utf8String? Name
        {
            get;
        }

        /// <summary>
        /// Gets the signature of the field.
        /// </summary>
        FieldSignature? Signature
        {
            get;
        }

        /// <summary>
        /// Resolves the reference to a field definition.
        /// </summary>
        /// <returns>The resolved field definition, or <c>null</c> if the field could not be resolved.</returns>
        /// <remarks>
        /// This method assumes the context module as the resolution context.
        /// </remarks>
        new FieldDefinition? Resolve();

        /// <summary>
        /// Resolves the reference to a field definition, assuming the provided module as resolution context.
        /// </summary>
        /// <param name="context">The module to assume as resolution context.</param>
        /// <returns>The resolved field definition, or <c>null</c> if the field could not be resolved.</returns>
        new FieldDefinition? Resolve(ModuleDefinition context);
    }
}
