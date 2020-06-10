namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Provides a base for all version tables stored in a native version resource file.
    /// </summary>
    public abstract class VersionTableEntry : SegmentBase
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public abstract string Key
        {
            get;
        }

        /// <summary>
        /// Gets the value type of the table.
        /// </summary>
        protected abstract ResourceValueType ValueType
        {
            get;
        }

        /// <summary>
        /// Gets the value segment.
        /// </summary>
        protected abstract ISegment Value
        {
            get;
        }
        
        /// <summary>
        /// Creates a new header for the table entry.
        /// </summary>
        /// <returns></returns>
        protected ResourceTableHeader CreateHeader()
        {
            return new ResourceTableHeader
            {
                Length = (ushort) GetPhysicalSize(),
                ValueLength = (ushort) Value.GetPhysicalSize(),
                Type = ValueType,
                Key = Key,
            };
        }
        
    }
}