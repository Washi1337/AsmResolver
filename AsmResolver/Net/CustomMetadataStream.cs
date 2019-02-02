namespace AsmResolver.Net
{
    public class CustomMetadataStream : MetadataStream
    {
        internal static CustomMetadataStream FromReadingContext(ReadingContext context)
        {
            long position = context.Reader.Position;
            return new CustomMetadataStream(context.Reader.ReadBytes((int) context.Reader.Length))
            {
                StartOffset = position
            };
        }

        public CustomMetadataStream()
        {
            Data = new byte[0];
        }

        public CustomMetadataStream(byte[] data)
        {
            Data = data;
        }

        public byte[] Data
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)Data.Length;
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteBytes(Data);
        }
    }
}