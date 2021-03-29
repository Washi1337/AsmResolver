namespace AsmResolver.PE.Exceptions
{
    /// <summary>
    /// Represents a single entry in an exception data directory of a portable executable (PE).
    /// </summary>
    public interface IRuntimeFunction
    {
        /// <summary>
        /// Gets the address to the beginning of the referenced function.
        /// </summary>
        ISegmentReference Begin
        {
            get;
        }

        /// <summary>
        /// Gets the address to the end of the referenced function.
        /// </summary>
        ISegmentReference End
        {
            get;
        }
    }
}
