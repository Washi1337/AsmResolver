using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class AssemblyProcessor : MetadataMember<MetadataRow<uint>>
    {
        public AssemblyProcessor(uint processor)
            : base(null, new MetadataToken(MetadataTokenType.AssemblyProcessor))
        {
        }
        
        internal AssemblyProcessor(MetadataImage image, MetadataRow<uint> row)
            : base(image, row.MetadataToken)
        {
            Processor = row.Column1;
        }

        public uint Processor
        {
            get;
            set;
        }
    }
}
