using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents the header to the .NET metadata in a windows assembly image.
    /// </summary>
    public class MetadataHeader : FileSegment
    {
        internal static MetadataHeader FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;

            var header = new MetadataHeader
            {
                StartOffset = reader.Position,

                Signature = reader.ReadUInt32(),
                MajorVersion = reader.ReadUInt16(),
                MinorVersion = reader.ReadUInt16(),
                Reserved = reader.ReadUInt32(),
                VersionLength = reader.ReadUInt32(),
            };

            header.VersionString = Encoding.ASCII.GetString(reader.ReadBytes((int)header.VersionLength));
            header.Flags = reader.ReadUInt16();
            var streamCount = reader.ReadUInt16();

            for (int i = 0; i < streamCount; i++)
            {
                var streamHeader = MetadataStreamHeader.FromReadingContext(context);
                header.StreamHeaders.Add(streamHeader);
            }

            return header;
        }

        private MetadataHeader()
        {
            StreamHeaders = new MetadataStreamHeaderCollection(this);
        }

        public MetadataHeader(ImageNetDirectory directory)
            : this()
        {
            NetDirectory = directory;
        }

        /// <summary>
        /// Gets the parent .NET data directory.
        /// </summary>
        public ImageNetDirectory NetDirectory
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the signature of the metadata header. Must be 0x424A5342 (BSJB).
        /// </summary>
        public uint Signature
        {
            get;
            set;
        }

        public ushort MajorVersion
        {
            get;
            set;
        }

        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Reserved. Must be zero.
        /// </summary>
        public uint Reserved
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the length of the <see cref="VersionString"/> field.
        /// </summary>
        public uint VersionLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version of the .NET framework that the .net assembly image targets.
        /// </summary>
        public string VersionString
        {
            get;
            set;
        }

        /// <summary>
        /// Reserved. Should be zero.
        /// </summary>
        public ushort Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a list of all stream headers defined in the metadata data directory.
        /// </summary>
        public IList<MetadataStreamHeader> StreamHeaders
        {
            get;
            private set;
        }
        
        // TODO
        /// <summary>
        /// Gets or sets the metadata resolver that will be used when <see cref="IResolvable.Resolve"/> is called on a specific member reference.
        /// </summary>
        //public IMetadataResolver MetadataResolver
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Gets all metadata heap streams defined in the metadata data directory.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MetadataStream> GetStreams()
        {
            return StreamHeaders.Select(x => x.Stream);
        } 

        /// <summary>
        /// Gets the first occuring metadata heap stream with the given name.
        /// </summary>
        /// <param name="name">The name of the stream to get.</param>
        /// <returns></returns>
        public MetadataStream GetStream(string name)
        {
            var header = StreamHeaders.FirstOrDefault(x => x.Name == name);
            return header != null ? header.Stream : null;
        }

        /// <summary>
        /// Gets the first occuring metadata heap stream that inherits from the given type argument.
        /// </summary>
        /// <typeparam name="TStream">The type of the metadata stream.</typeparam>
        /// <returns></returns>
        public TStream GetStream<TStream>()
            where TStream : MetadataStream
        {
            var header = StreamHeaders.FirstOrDefault(x => x.Stream is TStream);
            return header != null ? (TStream)header.Stream : null;
        }

        public MetadataImage LockMetadata()
        {
            var tableStream = GetStream<TableStream>();
            tableStream.IsReadOnly = true;
            return new MetadataImage(this);
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(1 * sizeof (uint) +
                          2 * sizeof (ushort) +
                          2 * sizeof (uint) +
                          VersionLength +
                          2 * sizeof (ushort) +
                          StreamHeaders.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;

            writer.WriteUInt32(Signature);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt32(Reserved);
            writer.WriteUInt32(VersionLength);
            writer.WriteAsciiString(VersionString);
            writer.WriteZeroes((int)(VersionLength - VersionString.Length));
            writer.WriteUInt16(Flags);
            writer.WriteUInt16((ushort)StreamHeaders.Count);

            foreach (var streamHeader in StreamHeaders)
                streamHeader.Write(context);
        }

    }

}