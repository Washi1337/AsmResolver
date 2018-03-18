namespace AsmResolver
{
    /// <summary>
    /// Represents the NT headers in a windows assembly image.
    /// </summary>
    public class ImageNtHeaders : FileSegment
    {
        private ImageOptionalHeader _optionalHeader;
        private ImageFileHeader _fileHeader;

        internal static ImageNtHeaders FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;

            return new ImageNtHeaders
            {
                StartOffset = reader.Position,
                Signature = reader.ReadUInt32(),
                FileHeader = ImageFileHeader.FromReadingContext(context),
                OptionalHeader = ImageOptionalHeader.FromReadingContext(context),
            };
        }

        /// <summary>
        /// Gets or sets the signature of the NT headers.
        /// </summary>
        public uint Signature
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the file header of the windows assembly image.
        /// </summary>
        public ImageFileHeader FileHeader
        {
            get { return _fileHeader ?? (_fileHeader = new ImageFileHeader()); }
            private set { _fileHeader = value; }
        }

        /// <summary>
        /// Gets the optional header (either 32-bit or 64-bit) of the windows assembly image.
        /// </summary>
        public ImageOptionalHeader OptionalHeader
        {
            get { return _optionalHeader ?? (_optionalHeader = new ImageOptionalHeader()); }
            private set { _optionalHeader = value; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (uint) + FileHeader.GetPhysicalLength() + OptionalHeader.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteUInt32(Signature);
            FileHeader.Write(context);
            OptionalHeader.Write(context);
        }
    }
}
