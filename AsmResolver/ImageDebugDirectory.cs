namespace AsmResolver
{
    /// <summary>
    /// Represents the debug data directory in a windows assembly image.
    /// </summary>
    public class ImageDebugDirectory : FileSegment
    {
        public static ImageDebugDirectory FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var directory =  new ImageDebugDirectory()
            {
                StartOffset = reader.StartPosition,
                Characteristics = reader.ReadUInt32(),
                TimeDateStamp = reader.ReadUInt32(),
                MajorVersion = reader.ReadUInt16(),
                MinorVersion = reader.ReadUInt16(),
                Type = (DebugInformationFormat)reader.ReadUInt32(),
                SizeOfData = reader.ReadUInt32(),
                AddressOfRawData = reader.ReadUInt32(),
                PointerToRawData = reader.ReadUInt32(),
            };
            directory._dataReadingContext = context.CreateSubContext(directory.PointerToRawData, (int)directory.SizeOfData);
            return directory;
        }

        private ReadingContext _dataReadingContext;
        private DataSegment _data;

        /// <summary>
        /// Gets or sets the characteristics of the directory.
        /// </summary>
        public uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the raw time stamp value of the directory.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version of the directory.
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the minor version of the directory.
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the format of the information of the directory.
        /// </summary>
        public DebugInformationFormat Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the data of the directory.
        /// </summary>
        public uint SizeOfData
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the relative virtual address of the raw data.
        /// </summary>
        public uint AddressOfRawData
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the absolute file offset of the raw data.
        /// </summary>
        public uint PointerToRawData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the debugging data.
        /// </summary>
        public DataSegment Data
        {
            get
            {
                if (_data != null || _dataReadingContext == null)
                    return _data;

                _data = new DataSegment(_dataReadingContext.Reader.ReadBytes((int)_dataReadingContext.Reader.Length));
                _dataReadingContext = null;
                return _data;
            }
            set
            {
                _data = value;
                _dataReadingContext = null;
            }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (uint) +
                   sizeof (uint) +
                   sizeof (ushort) +
                   sizeof (ushort) +
                   sizeof (uint) +
                   sizeof (uint) +
                   sizeof (uint) +
                   sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(Characteristics);
            writer.WriteUInt32(TimeDateStamp);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt32((uint)Type);
            writer.WriteUInt32(SizeOfData);
            writer.WriteUInt32(AddressOfRawData);
            writer.WriteUInt32(PointerToRawData);
        }
    }

    public enum DebugInformationFormat
    {
        Unknown,
        Coff,
        CodeView,
        FramePointerOmission,
        Miscellaneous,
        Exception,
        Fixup,
        Borland,
    }
}
