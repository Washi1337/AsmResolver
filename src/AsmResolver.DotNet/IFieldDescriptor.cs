using AsmResolver.DotNet.Blob;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a field in a managed assembly. 
    /// </summary>
    public interface IFieldDescriptor : IMemberDescriptor
    {
        /// <summary>
        /// Gets the signature of the field.
        /// </summary>
        FieldSignature Signature
        {
            get;
        }
    }
}