using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized
{
    public class SerializedLocalScope : LocalScope
    {
        private PdbReaderContext _context;
        private LocalScopeRow _row;

        public SerializedLocalScope(PdbReaderContext context, MetadataToken token, in LocalScopeRow row)
            : base(token)
        {
            _context = context;
            _row = row;
        }

        protected override MethodDefinition? GetOwner() => _context.OwningModule.TryLookupMember<MethodDefinition>(_row.Method, out var method) ? method : null;
    }
}
