using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single member in a .NET image.
    /// </summary>
    public abstract class MetadataMember : IMetadataMember
    {
        internal static readonly Utf8String NullName = "<<<NULL NAME>>>";

        /// <summary>
        /// Initializes the metadata member with a metadata token.
        /// </summary>
        /// <param name="token">The token.</param>
        protected MetadataMember(MetadataToken token)
        {
            MetadataToken = token;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            internal set;
        }
    }
}
