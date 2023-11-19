namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Represents a managed native header of a .NET module, containing Ahead-of-Time (AOT) compilation metadata.
    /// </summary>
    public interface IManagedNativeHeader : ISegment
    {
        /// <summary>
        /// Gets the signature of the native header, indicating the type of metadata that is stored.
        /// </summary>
        ManagedNativeHeaderSignature Signature
        {
            get;
        }
    }
}
