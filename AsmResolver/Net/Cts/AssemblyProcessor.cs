using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class AssemblyProcessor : MetadataMember<MetadataRow<uint>>
    {
        public AssemblyProcessor(uint processor)
            : base(new MetadataToken(MetadataTokenType.AssemblyProcessor))
        {
            Processor = processor;
        }
        
        internal AssemblyProcessor(MetadataImage image, MetadataRow<uint> row)
            : base(row.MetadataToken)
        {
            Assembly = image.Assembly;
            Processor = row.Column1;
        }

        public override MetadataImage Image
        {
            get { return Assembly != null ? Assembly.Image : null; }
        }

        public AssemblyDefinition Assembly
        {
            get;
            internal set;
        }

        public uint Processor
        {
            get;
            set;
        }
    }
}
