using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Maps new metadata tokens to members added to a .NET tables stream.
    /// </summary>
    public interface ITokenMapping
    {
        /// <summary>
        /// Gets the new metadata token assigned to the provided member.
        /// </summary>
        /// <param name="member">The member.</param>
        MetadataToken this[IMetadataMember member]
        {
            get;
        }
    }
}
