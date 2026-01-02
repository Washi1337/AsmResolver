using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized
{
    public partial class SerializedPortablePdb
    {
        private readonly CachedSerializedPdbMemberFactory _factory;
        private readonly LazyRidListRelation<LocalScopeRow> _localVariableLists;

        private OneToManyRelation<uint, uint>? _localScopes;
        private OneToManyRelation<uint, uint>? _importScopeChildren;

        [MemberNotNull(nameof(_localScopes))]
        private void EnsureLocalScopesInitialized()
        {
            if (_localScopes == null)
                Interlocked.CompareExchange(ref _localScopes,  InitializeLocalScopes(), null);
        }

        private OneToManyRelation<uint, uint> InitializeLocalScopes()
        {
            var tablesStream = PdbReaderContext.TablesStream;
            var scopeTable = tablesStream.GetTable<LocalScopeRow>();

            var localScopes = new OneToManyRelation<uint, uint>(scopeTable.Count);
            for (int i = 0; i < scopeTable.Count; i++)
            {
                var scopeRid = (uint) (i + 1);
                localScopes.Add(scopeTable[i].Method, scopeRid);
            }

            return localScopes;
        }

        internal uint GetLocalScopeOwner(uint localScopeRid)
        {
            EnsureLocalScopesInitialized();
            return _localScopes.GetKey(localScopeRid);
        }

        internal OneToManyRelation<uint, uint>.ValueSet GetLocalScopes(uint ownerMethodRid)
        {
            EnsureLocalScopesInitialized();
            return _localScopes.GetValues(ownerMethodRid);
        }

        [MemberNotNull(nameof(_importScopeChildren))]
        private void EnsureImportScopeChildrenInitialized()
        {
            if (_importScopeChildren == null)
                Interlocked.CompareExchange(ref _importScopeChildren, InitializeImportScopeChildren(), null);
        }

        private OneToManyRelation<uint, uint> InitializeImportScopeChildren()
        {
            var tablesStream = PdbReaderContext.TablesStream;
            var scopeTable = tablesStream.GetTable<ImportScopeRow>();

            var importScopeChildren = new OneToManyRelation<uint, uint>(scopeTable.Count);
            for (int i = 0; i < scopeTable.Count; i++)
            {
                var scopeRid = (uint) (i + 1);
                importScopeChildren.Add(scopeTable[i].Parent, scopeRid);
            }

            return importScopeChildren;
        }

        internal OneToManyRelation<uint, uint>.ValueSet GetImportScopeChildren(uint importScope)
        {
            EnsureImportScopeChildrenInitialized();
            return _importScopeChildren.GetValues(importScope);
        }

        internal MetadataRange GetLocalVariableRange(uint localScopeRid) => _localVariableLists.GetMemberRange(localScopeRid);

        internal uint GetLocalVariableOwner(uint localVariableRid) => _localVariableLists.GetMemberOwner(localVariableRid);
    }
}
