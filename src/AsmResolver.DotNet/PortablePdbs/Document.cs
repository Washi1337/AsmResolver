using System;
using System.Diagnostics;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs
{
    [DebuggerDisplay("{Name}")]
    public partial class Document : IMetadataMember, IOwnedCollectionElement<PortablePdb>
    {
        public Document() : this(new MetadataToken(TableIndex.Document, 0)) { }

        public Document(MetadataToken token)
        {
            MetadataToken = token;
        }

        public MetadataToken MetadataToken { get; }

        public PortablePdb? Owner
        {
            get;
            set;
        }

        [LazyProperty]
        public partial Utf8String? Name
        {
            get;
            set;
        }

        [LazyProperty]
        public partial Guid HashAlgorithm
        {
            get;
            set;
        }

        [LazyProperty]
        public partial byte[]? Hash
        {
            get;
            set;
        }

        [LazyProperty]
        public partial Guid Language
        {
            get;
            set;
        }

        protected virtual Utf8String? GetName() => null;

        protected virtual Guid GetHashAlgorithm() => Guid.Empty;

        protected virtual byte[]? GetHash() => null;

        protected virtual Guid GetLanguage() => Guid.Empty;
    }
}
