namespace AsmResolver
{
    public class PointerSegment : FileSegment
    {
        public PointerSegment(FileSegment reference, IOffsetConverter offsetConverter, bool is32Bit)
        {
            Reference = reference;
            OffsetConverter = offsetConverter;
            Is32Bit = is32Bit;
        }

        public FileSegment Reference
        {
            get;
            set;
        }

        public IOffsetConverter OffsetConverter
        {
            get;
            set;
        }

        public bool Is32Bit
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return Is32Bit ? 4u : 8u;
        }

        public override void Write(WritingContext context)
        {
            var rva = OffsetConverter.FileOffsetToRva(Reference.StartOffset);
            if (Is32Bit)
                context.Writer.WriteUInt32((uint)rva);
            else
                context.Writer.WriteUInt64((ulong)rva);
        }
    }
}
