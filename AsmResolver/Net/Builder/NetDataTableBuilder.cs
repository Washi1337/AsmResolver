using System.Collections.Generic;
using AsmResolver.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Builder
{
    public class NetDataTableBuilder : FileSegmentBuilder
    {
        private static readonly Dictionary<FieldRva, DataSegment> _dataSegments =
            new Dictionary<FieldRva, DataSegment>();

        public override void Build(BuildingContext context)
        {
            foreach (var fieldRva in context.Assembly.NetDirectory.
                MetadataHeader.GetStream<TableStream>().GetTable<FieldRva>())
            {
                GetFieldRvaSegment(fieldRva);
            }
            base.Build(context);
        }

        public DataSegment GetFieldRvaSegment(FieldRva fieldRva)
        {
            DataSegment segment;
            if (!_dataSegments.TryGetValue(fieldRva, out segment))
            {
                segment = new DataSegment(fieldRva.Data);
                Segments.Add(segment);
                _dataSegments.Add(fieldRva, segment);
            }
            return segment;
        }

        public override void UpdateReferences(BuildingContext context)
        {
            foreach (var fieldRva in context.Assembly.NetDirectory.
                MetadataHeader.GetStream<TableStream>().GetTable<FieldRva>())
            {
                fieldRva.Rva = fieldRva.MetadataRow.Column1 =
                    (uint)context.Assembly.FileOffsetToRva(GetFieldRvaSegment(fieldRva).StartOffset);
            }
            base.UpdateReferences(context);
        }
    }
}