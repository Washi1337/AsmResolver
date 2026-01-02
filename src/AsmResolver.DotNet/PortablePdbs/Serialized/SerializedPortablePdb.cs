using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized
{
    public partial class SerializedPortablePdb : PortablePdb
    {
        public SerializedPortablePdb(MetadataDirectory metadata, SerializedModuleDefinition owningModule) : base(owningModule)
        {
            PdbReaderContext = new PdbReaderContext(this, metadata, owningModule);
            _factory = new CachedSerializedPdbMemberFactory(PdbReaderContext);

            var pdbStream = PdbReaderContext.PdbStream;

            Array.Copy(pdbStream.Id, PdbId, 20);

            _localVariableLists = new LazyRidListRelation<LocalScopeRow>(PdbReaderContext.Metadata, TableIndex.LocalVariable, TableIndex.LocalScope,
                (rid, _) => rid, PdbReaderContext.TablesStream.GetLocalVariableRange);
        }

        public PdbReaderContext PdbReaderContext { get; }

        protected override IList<Document> GetDocuments()
        {
            var documentsTable = PdbReaderContext.TablesStream.GetTable<DocumentRow>(TableIndex.Document);

            var documents = new MemberCollection<PortablePdb, Document>(this, documentsTable.Count);

            for (int i = 0; i < documentsTable.Count; i++)
            {
                var rid = (uint)i + 1;
                documents.AddNoOwnerCheck(_factory.LookupDocument(new MetadataToken(TableIndex.Document, rid))!);
            }

            return documents;
        }

        public Document LookupDocument(MetadataToken token)
        {
            return _factory.LookupDocument(token)!;
        }

        public override bool TryLookupMember<T>(MetadataToken token, [MaybeNullWhen(false)] out T member) where T : class
        {
            if (_factory.TryLookupMember(token, out var metadataMember))
            {
                member = (T)metadataMember;
                return true;
            }

            member = null;
            return false;
        }
    }
}
