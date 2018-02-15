using System.Text;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a metadata storage stream header in a .NET assembly image.
    /// </summary>
    public class MetadataStreamHeader : FileSegment
    {
        internal static MetadataStreamHeader FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;

            var header = new MetadataStreamHeader
            {
                _readingContext = context,

                StartOffset = reader.Position,

                Offset = reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                Name = reader.ReadAlignedAsciiString(4),

            };

            return header;
        }

        private ReadingContext _readingContext;
        private MetadataStream _stream;

        private MetadataStreamHeader()
        {
        }

        public MetadataStreamHeader(string name)
            : this(name, new CustomMetadataStream())
        {
        }

        public MetadataStreamHeader(string name, MetadataStream stream)
        {
            Name = name;
            Stream = stream;
        }

        /// <summary>
        /// Gets or sets the offset to the raw data, relative to the start of the metadata data directory.
        /// </summary>
        public uint Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the raw data.
        /// </summary>
        public uint Size
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the metadata stream.
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the metadata stream associated with the header.
        /// </summary>
        public MetadataStream Stream
        {
            get
            {
                if (_stream != null)
                    return _stream;

                if (_readingContext == null)
                    _stream = new CustomMetadataStream();
                else
                {
                    var context =
                        _readingContext.CreateSubContext(
                            _readingContext.Assembly.NetDirectory.MetadataHeader.StartOffset + Offset, (int)Size);

                    switch (Name)
                    {
                        case "#-":
                        case "#~":
                            _stream = TableStream.FromReadingContext(context);
                            break;
                        case "#Strings":
                            _stream = StringStream.FromReadingContext(context);
                            break;
                        case "#US":
                            _stream = UserStringStream.FromReadingContext(context);
                            break;
                        case "#GUID":
                            _stream = GuidStream.FromReadingContext(context);
                            break;
                        case "#Blob":
                            _stream = BlobStream.FromReadingContext(context);
                            break;
                        default:
                            _stream = CustomMetadataStream.FromReadingContext(context);
                            break;
                    }
                }
                _stream.StreamHeader = this;
                return _stream;
            }
            set
            {
                if (_stream != null)
                    _stream.StreamHeader = null;
                _stream = value;
                if (value != null)
                    _stream.StreamHeader = this;
            }
        }

        /// <summary>
        /// Gets the metadata header the stream header is defined in.
        /// </summary>
        public MetadataHeader MetadataHeader
        {
            get;
            internal set;
        }

        public override uint GetPhysicalLength()
        {
            var length = Align((uint)(Encoding.ASCII.GetByteCount(Name) + 1), 4);
            return (uint)(2 * sizeof (uint) + length);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(Offset);
            writer.WriteUInt32(Size);

            var bytes = Encoding.ASCII.GetBytes(Name);
            writer.WriteBytes(bytes);
            int length = bytes.Length;
            do
            {
                context.Writer.WriteByte(0);
                length++;
            } while (length % 4 != 0);
        }
    }
}
