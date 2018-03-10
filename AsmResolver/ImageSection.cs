
using System;
using AsmResolver.Emit;

namespace AsmResolver
{
    public class ImageSection : SimpleFileSegmentBuilder, IOffsetConverter
    {
        public ImageSection(ImageSectionHeader header)
        {
            if (header == null)
                throw new ArgumentNullException("header");
            Header = header;
        }

        public ImageSection(ImageSectionHeader header, IBinaryStreamReader reader)
        {
            if (header == null)
                throw new ArgumentNullException("header");
            Header = header;
            Segments.Add(new DataSegment(reader.ReadBytes((int) reader.Length)));
        }

        public ImageSectionHeader Header
        {
            get;
            private set;
        }

        public long RvaToFileOffset(long rva)
        {
            return Header.RvaToFileOffset(rva);
        }

        public long FileOffsetToRva(long fileOffset)
        {
            return Header.FileOffsetToRva(fileOffset);
        }

        public uint GetVirtualSize()
        {
            return base.GetPhysicalLength();
        }
        
        public override uint GetPhysicalLength()
        {
            uint physicalLength = base.GetPhysicalLength();
            return Header != null && Header.Assembly != null
                ? Align(physicalLength, Header.Assembly.NtHeaders.OptionalHeader.FileAlignment)
                : physicalLength;
        }

        public override void Write(WritingContext context)
        {
            // TODO: more elegant way of creating buffer.
            uint physicalLength = GetPhysicalLength();
            context.Writer.Position += physicalLength;
            context.Writer.Position -= physicalLength;
            base.Write(context);
        }
    }
}
