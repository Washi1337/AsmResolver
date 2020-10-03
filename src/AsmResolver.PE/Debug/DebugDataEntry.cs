namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Represents a single entry in the debug data directory.
    /// </summary>
    public class DebugDataEntry : SegmentBase
    {
        /// <summary>
        /// Gets the static size of a single debug data entry header.
        /// </summary>
        public const uint DebugDataEntryHeaderSize =
                sizeof(uint) // Characteristics
                + sizeof(uint) // TimeDateStamp
                + sizeof(ushort) // MajorVersion
                + sizeof(ushort) // MinorVersion
                + sizeof(DebugDataType) // Type
                + sizeof(uint) // SizeOfData
                + sizeof(uint) // AddressOfRawData
                + sizeof(uint) // PointerToRawData
            ;

        private readonly LazyVariable<IDebugDataSegment> _contents;

        /// <summary>
        /// Initializes an empty <see cref="DebugDataEntry"/> instance.
        /// </summary>
        protected DebugDataEntry()
        {
            _contents = new LazyVariable<IDebugDataSegment>(GetContents);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DebugDataEntry"/> class.
        /// </summary>
        /// <param name="contents">The contents.</param>
        public DebugDataEntry(IDebugDataSegment contents)
            : this()
        {
            Contents = contents;
        }

        /// <summary>
        /// Reserved, must be zero. 
        /// </summary>
        public uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time and date that the debug data was created. 
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version number of the debug data format. 
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version number of the debug data format. 
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the raw contents of the debug data entry.
        /// </summary>
        public IDebugDataSegment Contents
        {
            get => _contents.Value;
            set => _contents.Value = value;
        }

        /// <summary>
        /// Obtains the contents of the entry.
        /// </summary>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Contents"/> property.
        /// </remarks>
        protected virtual IDebugDataSegment GetContents() => null;

        /// <inheritdoc />
        public override uint GetPhysicalSize() => DebugDataEntryHeaderSize;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(Characteristics);
            writer.WriteUInt32(TimeDateStamp);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt32((uint) (Contents?.Type ?? 0));
            writer.WriteUInt32(Contents?.GetPhysicalSize() ?? 0);
            writer.WriteUInt32(Contents?.Rva ?? 0);
            writer.WriteUInt32((uint) (Contents?.Offset ?? 0));
        }
    }
}