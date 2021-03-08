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

        /// <summary>
        /// Gets the new metadata token assigned to the provided member, if it was registered in this mapping.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="token">The new metadata token.</param>
        /// <returns><c>true</c> if the provided member was assigned a new token, <c>false</c> otherwise.</returns>
        bool TryGetNewToken(IMetadataMember member, out MetadataToken token);
    }
}
