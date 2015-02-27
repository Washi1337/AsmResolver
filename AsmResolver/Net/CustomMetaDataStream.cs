using System;

namespace AsmResolver.Net
{
    public class CustomMetaDataStream : MetadataStream
    {
        internal static CustomMetaDataStream FromReadingContext(ReadingContext context)
        {
            return new CustomMetaDataStream(context.Reader.ReadBytes((int)context.Reader.Length));
        }

        public CustomMetaDataStream()
        {
            Data = new byte[0];
        }

        public CustomMetaDataStream(byte[] data)
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