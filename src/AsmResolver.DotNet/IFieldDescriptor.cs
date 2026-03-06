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
        /// Imports the field using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported field.</returns>
        new IFieldDescriptor ImportWith(ReferenceImporter importer);

        /// <summary>
        /// Attempts to resolve the field reference to its definition, assuming the provided module as resolution context.
        /// </summary>
        /// <param name="context">The context to assume when resolving the field.</param>
        /// <param name="definition">The resolved field definition, or <c>null</c> if the field could not be resolved.</param>
        /// <returns>A value describing the success or failure status of the field resolution.</returns>
        ResolutionStatus Resolve(RuntimeContext? context, out FieldDefinition? definition);
    }
}
