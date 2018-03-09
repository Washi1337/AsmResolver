
namespace AsmResolver
{
    public class ImageSection : FileSegment, IOffsetConverter
    {
        private readonly LazyValue<FileSegment> _contents;
        
        public ImageSection()
        {
            _contents = new LazyValue<FileSegment>(default(FileSegment));
        }

        public ImageSection(IBinaryStreamReader reader)
        {
            _contents = new LazyValue<FileSegment>(() =>
                new DataSegment(reader.ReadBytes((int) reader.Length)));
        }

        public ImageSectionHeader Header
        {
            get;
            private set;
        }

        public FileSegment Contents
        {
            get { return _contents.Value; }
            set { _contents.Value = value; }
        }

        public long RvaToFileOffset(long rva)
        {
            return Header.RvaToFileOffset(rva);
        }

        public long FileOffsetToRva(long fileOffset)
        {
            return Header.FileOffsetToRva(fileOffset);
        }

        public override uint GetPhysicalLength()
        {
            uint physicalLength = Contents.GetPhysicalLength();
            return Header != null && Header.Assembly != null
                ? Align(physicalLength, Header.Assembly.NtHeaders.OptionalHeader.FileAlignment)
                : physicalLength;
        }

        public override void Write(WritingContext context)
        {
            // TODO: more elegant way of creating buffer.
            context.Writer.Position += GetPhysicalLength();
            context.Writer.Position -= GetPhysicalLength();
            Contents.Write(context);
        }
    }
}
