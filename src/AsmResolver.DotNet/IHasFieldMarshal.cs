using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member that can be assigned a field marshal descriptor, and can be referenced by a HasFieldMarshal
    /// coded index.
    /// </summary>
    public interface IHasFieldMarshal : IMetadataDefinition
    {
        /// <summary>
        /// Gets or sets the description on how a specific value needs to be marshaled upon calling to or from unmanaged
        /// code via P/Invoke dispatch.
        /// </summary>
        MarshalDescriptor? MarshalDescriptor
        {
            get;
            set;
        }
    }
}
