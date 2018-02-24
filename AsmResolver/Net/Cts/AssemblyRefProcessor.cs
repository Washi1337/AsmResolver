using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class AssemblyRefProcessor : MetadataMember<MetadataRow<uint,uint>>
    {
        private readonly LazyValue<AssemblyReference> _reference;

        public AssemblyRefProcessor(AssemblyReference reference, uint processor)
            : base(null, new MetadataToken(MetadataTokenType.AssemblyRefProcessor))
        {
            Processor = processor;
            _reference.Value = reference;
        }

        internal AssemblyRefProcessor(MetadataImage image, MetadataRow<uint, uint> row)
            : base(image, row.MetadataToken)
        {
            Processor = row.Column1;
            _reference = new LazyValue<AssemblyReference>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.AssemblyRef);
                MetadataRow referenceRow;
                return table.TryGetRow((int) (row.Column2 - 1), out referenceRow)
                    ? (AssemblyReference) table.GetMemberFromRow(image, referenceRow)
                    : null;
            });
        }

        public uint Processor
        {
            get;
            set;
        }

        public AssemblyReference Reference
        {
            get { return _reference.Value; }
            set { _reference.Value = value; }
        }

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            buffer.TableStreamBuffer.GetTable<AssemblyRefProcessorTable>().Add(new MetadataRow<uint, uint>
            {
                Column1 = Processor,
                Column2 = Reference.MetadataToken.Rid,
            });
        }
    }
}
