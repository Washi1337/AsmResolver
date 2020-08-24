namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Represents a single entry in the debug data directory.
    /// </summary>
    public class DebugDataEntry
    {
        private readonly LazyVariable<ISegment> _contents;

        /// <summary>
        /// Initializes an empty <see cref="DebugDataEntry"/> instance.
        /// </summary>
        protected DebugDataEntry()
        {
            _contents = new LazyVariable<ISegment>(GetContents);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DebugDataEntry"/> class.
        /// </summary>
        /// <param name="type">The type of data to store.</param>
        /// <param name="contents">The contents.</param>
        public DebugDataEntry(DebugDataType type, ISegment contents)
            : this()
        {
            Type = type;
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
        /// Gets or sets the format of debugging information stored in <see cref="Contents"/>.
        /// </summary>
        public DebugDataType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw contents of the debug data entry.
        /// </summary>
        public ISegment Contents
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
        protected virtual ISegment GetContents() => null;
    }
}