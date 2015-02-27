using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net
{
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
            : this(name, new CustomMetaDataStream())
        {
        }

        public MetadataStreamHeader(string name, MetadataStream stream)
        {
            Name = name;
            Stream = stream;
        }

        public uint Offset
        {
            get;
            set;
        }

        public uint Size
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public MetadataStream Stream
        {
            get
            {
                if (_stream != null)
                    return _stream;

                if (_readingContext == null)
                    _stream = new CustomMetaDataStream();
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
                            _stream = CustomMetaDataStream.FromReadingContext(context);
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

        public MetadataHeader MetaDataHeader
        {
            get;
            internal set;
        }

        public override uint GetPhysicalLength()
        {
            var length = Align((uint)(Encoding.ASCII.GetByteCount(Name) + 1), 4);
            //if (length < 4)
            //    length = 4;
            //else if (length < 8)
            //    length = 8;
            //else if (length < 12)
            //    length = 12;
            //else if (length < 16)
            //    length = 16;
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
