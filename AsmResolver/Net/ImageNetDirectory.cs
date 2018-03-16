namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a .NET data directory header (COR20 header) in a windows assembly image.
    /// </summary>
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
                MetadataDirectory = ImageDataDirectory.FromReadingContext(context),
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
        private DataSegment _strongNameData;

        private ResourcesManifest _resources;
        private VTablesDirectory _vtablesDirectory;

        public ImageNetDirectory()
        {
            MetadataDirectory = new ImageDataDirectory();
            ResourcesDirectory = new ImageDataDirectory();
            StrongNameSignatureDirectory = new ImageDataDirectory();
            CodeManagerTableDirectory = new ImageDataDirectory();
            VTableFixupsDirectory = new ImageDataDirectory();
            ExportAddressTableJumpsDirectory = new ImageDataDirectory();
            ManagedNativeHeaderDirectory = new ImageDataDirectory();
        }

        /// <summary>
        /// Gets the assembly defining the .NET header.
        /// </summary>
        public WindowsAssembly Assembly
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the size of the .NET data directory header.
        /// </summary>
        public uint Cb
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major runtime version number.
        /// </summary>
        public ushort MajorRuntimeVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor runtime version number.
        /// </summary>
        public ushort MinorRuntimeVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the metadata data directory header.
        /// </summary>
        public ImageDataDirectory MetadataDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the attributes of the .NET data directory, specifying properties of the .net assembly image.
        /// </summary>
        public ImageNetDirectoryFlags Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the metadata token to the managed entrypoint or relative virtual address of the unmanaged entrypoint.
        /// </summary>
        public uint EntryPointToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the managed resources data directory header.
        /// </summary>
        public ImageDataDirectory ResourcesDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the strong name signature data directory header.
        /// </summary>
        public ImageDataDirectory StrongNameSignatureDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the code manager table data directory header.
        /// </summary>
        public ImageDataDirectory CodeManagerTableDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the custom vtable fixups data directory header.
        /// </summary>
        public ImageDataDirectory VTableFixupsDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the export address table data directory header.
        /// </summary>
        public ImageDataDirectory ExportAddressTableJumpsDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the managed native header data directory header.
        /// </summary>
        public ImageDataDirectory ManagedNativeHeaderDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the metadata header of the .NET data directory.
        /// </summary>
        public MetadataHeader MetadataHeader
        {
            get
            {
                if (_metaDataHeader != null)
                    return _metaDataHeader;

                if (_readingContext == null)
                    return _metaDataHeader = new MetadataHeader(this);

                var context = _readingContext.CreateSubContext(
                    _readingContext.Assembly.RvaToFileOffset(MetadataDirectory.VirtualAddress));
                if (context == null)
                    return _metaDataHeader = new MetadataHeader(this);
                
                _metaDataHeader = MetadataHeader.FromReadingContext(context);
                _metaDataHeader.NetDirectory = this;
                return _metaDataHeader;
            }
        }

        /// <summary>
        /// Gets or sets the strong name of the .NET assembly image.
        /// </summary>
        public DataSegment StrongNameData
        {
            get
            {
                if (_strongNameData != null || _readingContext == null || StrongNameSignatureDirectory.VirtualAddress == 0)
                    return _strongNameData;

                var context = _readingContext.CreateSubContext(
                    _readingContext.Assembly.RvaToFileOffset(StrongNameSignatureDirectory.VirtualAddress),
                    (int) StrongNameSignatureDirectory.Size);
                return _strongNameData = DataSegment.FromReader(context.Reader);
            }
            set { _strongNameData = value; }
        }

        /// <summary>
        /// Gets or sets the managed resources manifest directory of the .NET assembly image.
        /// </summary>
        public ResourcesManifest ResourcesManifest
        {
            get
            {
                if (_resources != null || _readingContext == null || ResourcesDirectory.VirtualAddress == 0)
                    return _resources;
                
                var context = _readingContext.CreateSubContext(
                    _readingContext.Assembly.RvaToFileOffset(ResourcesDirectory.VirtualAddress),
                    (int) ResourcesDirectory.Size);
                return _resources = ResourcesManifest.FromReadingContext(context);
            }
            set { _resources = value; }
        }
        
        /// <summary>
        /// Gets or sets the VTable fixups directory of the .NET assembly image.
        /// </summary>
        public VTablesDirectory VTablesDirectory
        {
            get
            {
                if (_vtablesDirectory != null)
                    return _vtablesDirectory;

                if (_readingContext != null && VTableFixupsDirectory.VirtualAddress != 0)
                {
                    var context = _readingContext.CreateSubContext(
                        _readingContext.Assembly.RvaToFileOffset(VTableFixupsDirectory.VirtualAddress), (int) VTableFixupsDirectory.Size);
                    if (context != null)
                        return _vtablesDirectory = VTablesDirectory.FromReadingContext(context);
                }

                return null;
            }
            set { _vtablesDirectory = value; }
        }

        public override uint GetPhysicalLength()
        {
            var dirLength = MetadataDirectory.GetPhysicalLength();
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
            MetadataDirectory.Write(context);
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
