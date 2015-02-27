using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    public class MetadataHeader : FileSegment
    {
        private TypeSystem _typeSystem;

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
            MetadataResolver = new DefaultMetadataResolver(new DefaultNetAssemblyResolver());
            StreamHeaders = new MetadataStreamHeaderCollection(this);
        }

        public MetadataHeader(ImageNetDirectory directory)
            : this()
        {
            NetDirectory = directory;
        }

        public ImageNetDirectory NetDirectory
        {
            get;
            internal set;
        }

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

        public uint Reserved
        {
            get;
            set;
        }

        public uint VersionLength
        {
            get;
            set;
        }

        public string VersionString
        {
            get;
            set;
        }

        public ushort Flags
        {
            get;
            set;
        }

        public IList<MetadataStreamHeader> StreamHeaders
        {
            get;
            private set;
        }

        public TypeSystem TypeSystem
        {
            get
            {
                return _typeSystem ?? (_typeSystem = new TypeSystem(this,
                    GetStream<TableStream>().GetTable<AssemblyDefinition>()[0].Name == "mscorlib"));
            }
        }

        public IMetadataResolver MetadataResolver
        {
            get;
            set;
        }

        public MetadataStream GetStream(string name)
        {
            return StreamHeaders.Single(x => x.Name == name).Stream;
        }

        public TStream GetStream<TStream>()
            where TStream : MetadataStream
        {
            return (TStream)StreamHeaders.Single(x => x.Stream is TStream).Stream;
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