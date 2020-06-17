using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single member in a .NET image. 
    /// </summary>
    public interface IMetadataMember
    {
        /// <summary>
        /// Gets the metadata token of the member.
        /// </summary>
        MetadataToken MetadataToken
        {
            get;
            internal set;
        }
    }
}