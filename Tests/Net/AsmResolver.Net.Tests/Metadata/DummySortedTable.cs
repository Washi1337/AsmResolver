using System;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Tests.Net.Metadata
{
    public class DummySortedTable : SortedMetadataTable<MetadataRow<uint>>
    {
        public DummySortedTable()
            : base(0)
        {
        }

        public override MetadataTokenType TokenType => 0;

        public override uint ElementByteCount => 0;

        protected override MetadataRow<uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            throw new NotSupportedException();
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint> row)
        {
            throw new NotSupportedException();
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint> row)
        {
            throw new NotSupportedException();
        }
    }
}