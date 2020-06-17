using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    public abstract class MetadataMember : IMetadataMember
    {
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