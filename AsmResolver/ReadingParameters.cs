namespace AsmResolver
{
    /// <summary>
    /// Provides configurable parameters used for reading an assembly image.
    /// </summary>
    public class ReadingParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether the amount of data directories defined in the optional header of the assembly image
        /// should be ignored and <see cref="DataDirectoryCount"/> should be used instead.
        /// </summary>
        public bool ForceDataDirectoryCount
        {
            get;
            set;
        }

        /// <summary>
        /// When enabled by <see cref="ForceDataDirectoryCount"/>, gets or sets the amount of data directories to read from the assembly image.
        /// </summary>
        public int DataDirectoryCount
        {
            get;
            set;
        }
    }
}
