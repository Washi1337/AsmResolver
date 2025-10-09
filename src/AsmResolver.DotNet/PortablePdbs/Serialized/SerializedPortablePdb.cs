using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized
{
    public class SerializedPortablePdb : PortablePdb
    {
        private readonly PdbReaderContext _context;
        private readonly CachedSerializedPdbMemberFactory _factory;

        public SerializedPortablePdb(MetadataDirectory metadata, ModuleDefinition? owningModule) : base(owningModule)
        {
            _context = new PdbReaderContext(this, metadata, owningModule);
            _factory = new CachedSerializedPdbMemberFactory(_context);

            var pdbStream = _context.PdbStream;

            Array.Copy(pdbStream.Id, PdbId, 20);
            EntryPointToken = pdbStream.EntryPoint;
            if (_context.OwningModule?.TryLookupMember<MethodDefinition>(EntryPointToken, out var method) is true)
            {
                EntryPoint = method;
            }
        }

        protected override IList<Document> GetDocuments()
        {
            var documentsTable = _context.TablesStream.GetTable<DocumentRow>(TableIndex.Document);

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

        public override bool TryLookupMember<T>(MetadataToken token, [NotNullWhen(true)] out T? member) where T : class
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
