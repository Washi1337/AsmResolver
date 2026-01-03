using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
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

        protected override IList<LocalVariable> GetLocalVariables()
        {
            var range = _context.Pdb.GetLocalVariableRange(MetadataToken.Rid);
            var localVariables = new MemberCollection<LocalScope, LocalVariable>(this, range.Count);

            foreach (var token in range)
            {
                if (_context.Pdb.TryLookupMember<LocalVariable>(token, out var variable))
                {
                    localVariables.AddNoOwnerCheck(variable);
                }
            }

            return localVariables;
        }

        protected override ImportScope? GetImportScope() => _context.Pdb.TryLookupMember<ImportScope>(new MetadataToken(TableIndex.ImportScope, _row.ImportScope), out var importScope) ? importScope : null;

        protected override IList<LocalConstant> GetLocalConstants()
        {
            var range = _context.Pdb.GetLocalConstantRange(MetadataToken.Rid);
            var localConstants = new MemberCollection<LocalScope, LocalConstant>(this, range.Count);

            foreach (var token in range)
            {
                if (_context.Pdb.TryLookupMember<LocalConstant>(token, out var variable))
                {
                    localConstants.AddNoOwnerCheck(variable);
                }
            }

            return localConstants;
        }
    }
}
