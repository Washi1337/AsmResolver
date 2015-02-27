namespace AsmResolver
{
    public class ImageResourceDataEntry : FileSegment
    {
        internal static ImageResourceDataEntry FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var entry = new ImageResourceDataEntry
            {
                StartOffset = reader.Position,
                OffsetToData = reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                CodePage = reader.ReadUInt32(),
                Reserved = reader.ReadUInt32(),

            };

            entry._dataReaderContext = context.CreateSubContext(context.Assembly.RvaToFileOffset(entry.OffsetToData));

            return entry;
        }

        private ReadingContext _dataReaderContext;
        private byte[] _data;

        private ImageResourceDataEntry()
        {
        }

        public ImageResourceDataEntry(byte[] data)
        {
            Data = data;
        }

        public uint OffsetToData
        {
            get;
            set;
        }

        public uint Size
        {
            get;
            set;
        }

        public uint CodePage
        {
            get;
            set;
        }

        public uint Reserved
        {
            get;
            set;
        }

        public byte[] Data
        {
            get
            {
                if (_data != null)
                    return _data;
                if (_dataReaderContext != null)
                {
                    _data = _dataReaderContext.Reader.ReadBytes((int)Size);
                    _dataReaderContext = null;
                }
                return _data;
            }
            set
            {
                _data = value;
                _dataReaderContext = null;
            }
        }

        public override uint GetPhysicalLength()
        {
            return 4 * sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(OffsetToData);
            writer.WriteUInt32(Size);
            writer.WriteUInt32(CodePage);
            writer.WriteUInt32(Reserved);
        }

    }
}