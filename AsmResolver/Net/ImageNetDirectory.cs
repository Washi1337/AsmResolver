using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net
{
    public class ImageNetDirectory : FileSegment
    {
        internal static ImageNetDirectory FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;

            var directory = new ImageNetDirectory
            {
                _readingContext = context,

                StartOffset = reader.Position,

                Cb = reader.ReadUInt32(),
                MajorRuntimeVersion = reader.ReadUInt16(),
                MinorRuntimeVersion = reader.ReadUInt16(),
                MetaDataDirectory = ImageDataDirectory.FromReadingContext(context),
                Flags = (ImageNetDirectoryFlags)reader.ReadUInt32(),
                EntryPointToken = reader.ReadUInt32(),
                ResourcesDirectory = ImageDataDirectory.FromReadingContext(context),
                StrongNameSignatureDirectory = ImageDataDirectory.FromReadingContext(context),
                CodeManagerTableDirectory = ImageDataDirectory.FromReadingContext(context),
                VTableFixupsDirectory = ImageDataDirectory.FromReadingContext(context),
                ExportAddressTableJumpsDirectory = ImageDataDirectory.FromReadingContext(context),
                ManagedNativeHeaderDirectory = ImageDataDirectory.FromReadingContext(context),

            };
            
            return directory;
        }

        private ReadingContext _readingContext;
        private MetadataHeader _metaDataHeader;

        public ImageNetDirectory()
        {
            MetaDataDirectory = new ImageDataDirectory();
            ResourcesDirectory = new ImageDataDirectory();
            StrongNameSignatureDirectory = new ImageDataDirectory();
            CodeManagerTableDirectory = new ImageDataDirectory();
            VTableFixupsDirectory = new ImageDataDirectory();
            ExportAddressTableJumpsDirectory = new ImageDataDirectory();
            ManagedNativeHeaderDirectory = new ImageDataDirectory();
        }

        public WindowsAssembly Assembly
        {
            get;
            internal set;
        }

        public uint Cb
        {
            get;
            set;
        }

        public ushort MajorRuntimeVersion
        {
            get;
            set;
        }

        public ushort MinorRuntimeVersion
        {
            get;
            set;
        }

        public ImageDataDirectory MetaDataDirectory
        {
            get;
            private set;
        }

        public ImageNetDirectoryFlags Flags
        {
            get;
            set;
        }

        public uint EntryPointToken
        {
            get;
            set;
        }

        public ImageDataDirectory ResourcesDirectory
        {
            get;
            private set;
        }

        public ImageDataDirectory StrongNameSignatureDirectory
        {
            get;
            private set;
        }

        public ImageDataDirectory CodeManagerTableDirectory
        {
            get;
            private set;
        }

        public ImageDataDirectory VTableFixupsDirectory
        {
            get;
            private set;
        }

        public ImageDataDirectory ExportAddressTableJumpsDirectory
        {
            get;
            private set;
        }

        public ImageDataDirectory ManagedNativeHeaderDirectory
        {
            get;
            private set;
        }

        public MetadataHeader MetadataHeader
        {
            get
            {
                if (_metaDataHeader != null)
                    return _metaDataHeader;

                if (_readingContext == null)
                    return _metaDataHeader = new MetadataHeader(this);

                var context =
                    _readingContext.CreateSubContext(
                        _readingContext.Assembly.RvaToFileOffset(MetaDataDirectory.VirtualAddress));
                if (context == null)
                    return _metaDataHeader = new MetadataHeader(this);
                
                _metaDataHeader = MetadataHeader.FromReadingContext(context);
                _metaDataHeader.NetDirectory = this;
                return _metaDataHeader;
            }
        }

        public byte[] GetResourceData(uint offset)
        {
            if (_readingContext == null || ResourcesDirectory.VirtualAddress == 0)
                return null;

            var context = _readingContext.CreateSubContext(
                Assembly.RvaToFileOffset(ResourcesDirectory.VirtualAddress) + offset,
                (int)ResourcesDirectory.Size);

            if (context == null)
                return null;

            var length = context.Reader.ReadInt32();
            return context.Reader.ReadBytes(length);
        }

        public override uint GetPhysicalLength()
        {
            var dirLength = MetaDataDirectory.GetPhysicalLength();
            return 1 * sizeof (uint) +
                   2 * sizeof (ushort) +
                   1 * dirLength +
                   2 * sizeof (uint) +
                   6 * dirLength;
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(Cb);
            writer.WriteUInt16(MajorRuntimeVersion);
            writer.WriteUInt16(MinorRuntimeVersion);
            MetaDataDirectory.Write(context);
            writer.WriteUInt32((uint)Flags);
            writer.WriteUInt32(EntryPointToken);
            ResourcesDirectory.Write(context);
            StrongNameSignatureDirectory.Write(context);
            CodeManagerTableDirectory.Write(context);
            VTableFixupsDirectory.Write(context);
            ExportAddressTableJumpsDirectory.Write(context);
            ManagedNativeHeaderDirectory.Write(context);
        }
    }
}
