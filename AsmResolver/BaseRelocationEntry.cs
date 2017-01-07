namespace AsmResolver
{
    /// <summary>
    /// Represents a fixup entry in a base relocation block.
    /// </summary>
    public class BaseRelocationEntry : FileSegment
    {
        internal static BaseRelocationEntry FromReadingContext(ReadingContext context)
        {
            long offset = context.Reader.Position;
            var value = context.Reader.ReadUInt16();
            return new BaseRelocationEntry((BaseRelocationType) (value >> 12), (ushort) (value & 0x0FFF))
            {
                StartOffset = offset
            };
        }

        public BaseRelocationEntry(BaseRelocationType type, ushort offset)
        {
            Type = type;
            Offset = offset;
        }

        /// <summary>
        /// Gets or sets the type of fixup to be applied.
        /// </summary>
        public BaseRelocationType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the offset relative to the <see cref="BaseRelocationBlock.PageRva"/> where the fixup is to be applied. 
        /// </summary>
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