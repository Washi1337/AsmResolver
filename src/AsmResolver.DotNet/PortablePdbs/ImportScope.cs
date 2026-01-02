using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs
{
    public partial class ImportScope : IMetadataMember, IOwnedCollectionElement<ImportScope>
    {
        public ImportScope(MetadataToken token)
        {
            MetadataToken = token;
        }

        public MetadataToken MetadataToken
        {
            get;
        }

        [LazyProperty]
        public partial ImportScope? Parent
        {
            get;
            private set;
        }

        [LazyProperty]
        public partial IList<ImportScope> Children
        {
            get;
        }

        [LazyProperty]
        public partial ImportsCollection Imports
        {
            get;
        }

        ImportScope? IOwnedCollectionElement<ImportScope>.Owner
        {
            get => Parent;
            set => Parent = value;
        }

        protected virtual ImportScope? GetParent() => null;

        protected virtual IList<ImportScope> GetChildren() => new MemberCollection<ImportScope, ImportScope>(this);

        protected virtual ImportsCollection GetImports() => new();
    }
}
