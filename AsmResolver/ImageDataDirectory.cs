namespace AsmResolver
{
    /// <summary>
    /// Represents a data directory header in a windows assembly image.
    /// </summary>
    public class ImageDataDirectory : FileSegment
    {
        public const int ExportDirectoryIndex = 0;
        public const int ImportDirectoryIndex = 1;
        public const int ResourceDirectoryIndex = 2;
        public const int ExceptionDirectoryIndex = 3;
        public const int CertificateDirectoryIndex = 4;
        public const int BaseRelocationDirectoryIndex = 5;
        public const int DebugDirectoryIndex = 6;
        public const int ArchitectureDirectoryIndex = 7;
        public const int GlobalPtrDirectoryIndex = 8;
        public const int TlsDirectoryIndex = 9;
        public const int LoadConfigDirectoryIndex = 10;
        public const int BoundImportDirectoryIndex = 11;
        public const int IatDirectoryIndex = 12;
        public const int DelayImportDescrDirectoryIndex = 13;
        public const int ClrDirectoryIndex = 14;
        public const int ReservedDirectoryIndex = 15;

        internal static ImageDataDirectory FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            return new ImageDataDirectory
            {
                StartOffset = reader.Position,
                VirtualAddress = reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
            };
        }

        /// <summary>
        /// Gets or sets the relative virtual address (RVA) of the directory.
        /// </summary>
        public uint VirtualAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the directory.
        /// </summary>
        public uint Size
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return 2 * sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var stream = context.Writer;
            stream.WriteUInt32(VirtualAddress);
            stream.WriteUInt32(Size);
        }
    }
}
