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
                StartOffset = reader.Position,

                Offset = reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                Name = reader.ReadAlignedAsciiString(4),
            };
            
            header._stream = new LazyValue<MetadataStream>(() =>
            {
                var mdHeader = context.Assembly.NetDirectory.MetadataHeader;
                var stream = mdHeader.StreamParser.ReadStream(header.Name,
                    context.CreateSubContext(mdHeader.StartOffset + header.Offset, (int) header.Size));
                stream.StreamHeader = header;
                return stream;
            });

            return header;
        }

        private LazyValue<MetadataStream> _stream;

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
            _stream = new LazyValue<MetadataStream>(stream);
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
            get => _stream.Value;
            set
            {
                if (_stream.Value != null)
                    _stream.Value.StreamHeader = null;
                _stream.Value = value;
                if (value != null)
                    value.StreamHeader = this;
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
            return 2 * sizeof (uint) + length;
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
