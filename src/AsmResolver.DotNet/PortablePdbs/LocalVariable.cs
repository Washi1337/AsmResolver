using System.Diagnostics;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs
{
    [DebuggerDisplay("{Name,nq}")]
    public partial class LocalVariable : IMetadataMember, IOwnedCollectionElement<LocalScope>
    {
        public LocalVariable(MetadataToken token)
        {
            MetadataToken = token;
        }

        public MetadataToken MetadataToken
        {
            get;
        }

        [LazyProperty]
        public partial LocalScope? Scope
        {
            get;
            set;
        }

        LocalScope? IOwnedCollectionElement<LocalScope>.Owner
        {
            get => Scope;
            set => Scope = value;
        }

        public LocalVariableAttributes Attributes
        {
            get;
            set;
        }

        public int Index
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

        public bool IsDebuggerHidden
        {
            get => (Attributes & LocalVariableAttributes.DebuggerHidden) != 0;
            set => Attributes = (Attributes & ~LocalVariableAttributes.DebuggerHidden) | (value ? LocalVariableAttributes.DebuggerHidden : 0);
        }

        protected virtual LocalScope? GetScope() => null;

        protected virtual Utf8String? GetName() => null;
    }
}
