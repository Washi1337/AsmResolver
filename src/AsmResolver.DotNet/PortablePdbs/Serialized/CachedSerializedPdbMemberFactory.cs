using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized
{
    internal sealed class CachedSerializedPdbMemberFactory
    {
        private readonly PdbReaderContext _context;
        private readonly TablesStream _tablesStream;

        private Document?[]? _documents;
        private MethodDebugInformation?[]? _methodDebugInformations;

        internal CachedSerializedPdbMemberFactory(PdbReaderContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tablesStream = context.TablesStream;
        }

        internal bool TryLookupMember(MetadataToken token, [NotNullWhen(true)] out IMetadataMember? member)
        {
            member = token.Table switch
            {
                TableIndex.Document => LookupDocument(token),
                TableIndex.MethodDebugInformation => LookupMethodDebugInformation(token),
                _ => null,
            };

            return member is not null;
        }

        internal Document? LookupDocument(MetadataToken token)
        {
            return LookupOrCreateMember<Document, DocumentRow>(ref _documents, token,
                (c, t, r) => new SerializedDocument(c, t, r));
        }

        internal MethodDebugInformation? LookupMethodDebugInformation(MetadataToken token)
        {
            return LookupOrCreateMember<MethodDebugInformation, MethodDebugInformationRow>(ref _methodDebugInformations, token,
                (c, t, r) => new SerializedMethodDebugInformation(c, t, r));
        }

        internal TMember? LookupOrCreateMember<TMember, TRow>(ref TMember?[]? cache, MetadataToken token,
            Func<PdbReaderContext, MetadataToken, TRow, TMember?> createMember)
            where TRow : struct, IMetadataRow
            where TMember : class, IMetadataMember
        {
            // Obtain table.
            var table = (MetadataTable<TRow>) _tablesStream.GetTable(token.Table);

            // Check if within bounds.
            if (token.Rid == 0 || token.Rid > table.Count)
                return null;

            // Allocate cache if necessary.
            if (cache is null)
                Interlocked.CompareExchange(ref cache, new TMember[table.Count], null);

            // Get or create cached member.
            int index = (int) token.Rid - 1;
            var member = cache[index];
            if (member is null)
            {
                member = createMember(_context, token, table[index]);
                member = Interlocked.CompareExchange(ref cache[index], member, null)
                    ?? member;
            }

            return member;
        }
    }
}
