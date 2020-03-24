namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// When derived from this class, provides a description on how a specific type needs to be marshaled upon
    /// calling to or from unmanaged code via P/Invoke dispatch.
    /// </summary>
    public abstract class MarshalDescriptor : ExtendableBlobSignature
    {
        /// <summary>
        /// Gets the native type of the marshal descriptor. This is the byte any descriptor starts with. 
        /// </summary>
        public abstract NativeType NativeType
        {
            get;
        }
    }
}