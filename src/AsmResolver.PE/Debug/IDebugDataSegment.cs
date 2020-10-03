namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Represents a segment referenced by an entry in a debug data directory.
    /// </summary>
    public interface IDebugDataSegment : ISegment
    {
        /// <summary>
        /// Gets the type of debug data stored in the segment.
        /// </summary>
        DebugDataType Type
        {
            get;
        }
    }
}