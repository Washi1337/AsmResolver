namespace AsmResolver
{
    public class RvaDataSegment : FileSegment
    {
        private readonly byte[] _contents;

        public RvaDataSegment(uint rva, IBinaryStreamReader reader)
        {
            Rva = rva;
            _contents = reader.ReadBytes((int) reader.Length);
        }
        
        public RvaDataSegment(byte[] contents)
        {
            _contents = contents;
        }
        
        public uint Rva
        {
            get;
            set;
        }

        public bool ShouldBeAligned
        {
            get;
            set;
        }

        public IBinaryStreamReader CreateReader()
        {
            return new MemoryStreamReader(_contents);
        }
        
        public override uint GetPhysicalLength()
        {
            return (uint) _contents.Length;
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteBytes(_contents);
        }
    }
}