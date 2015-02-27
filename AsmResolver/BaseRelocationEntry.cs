namespace AsmResolver
{
    public class BaseRelocationEntry : FileSegment
    {
        internal static BaseRelocationEntry FromReadingContext(ReadingContext context)
        {
            var value = context.Reader.ReadUInt16();
            return new BaseRelocationEntry((BaseRelocationType)(value >> 12), (ushort)(value & 0x0FFF));
        }

        public BaseRelocationEntry(BaseRelocationType type, ushort offset)
        {
            Type = type;
            Offset = offset;
        }

        public BaseRelocationType Type
        {
            get;
            set;
        }

        public ushort Offset
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (ushort);
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteUInt16((ushort)((ushort)Type << 12 | (Offset & 0x0FFF)));
        }
    }
}