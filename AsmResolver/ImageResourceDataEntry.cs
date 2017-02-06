namespace AsmResolver
{
    /// <summary>
    /// Represents a data entry in a resource directory of a windows assembly image.
    /// </summary>
    public class ImageResourceDataEntry : FileSegment
    {
        internal static ImageResourceDataEntry FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var entry = new ImageResourceDataEntry
            {
                StartOffset = reader.Position,
                OffsetToData = reader.ReadUInt32(),
                _size = reader.ReadUInt32(),
                CodePage = reader.ReadUInt32(),
                Reserved = reader.ReadUInt32(),

            };

            entry._dataReaderContext = context.CreateSubContext(context.Assembly.RvaToFileOffset(entry.OffsetToData));

            return entry;
        }

        private ReadingContext _dataReaderContext;
        private byte[] _data;
        private uint _size;

        private ImageResourceDataEntry()
        {
        }

        public ImageResourceDataEntry(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets or sets the offset to the data, relative to the start of the resource data directory.
        /// </summary>
        public uint OffsetToData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size (in bytes) of the data.
        /// </summary>
        public uint Size
        {
            get { return _data == null ? _size : (uint) _data.Length; }
            set { _size = value; }
        }

        /// <summary>
        /// Gets or sets the code page used to decode code point values within the resource data. Typically, the code page would be the Unicode code page.
        /// </summary>
        public uint CodePage
        {
            get;
            set;
        }

        /// <summary>
        /// Reserved, must be zero.
        /// </summary>
        public uint Reserved
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data in the data entry.
        /// </summary>
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