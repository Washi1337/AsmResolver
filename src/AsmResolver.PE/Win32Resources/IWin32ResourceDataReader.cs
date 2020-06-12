namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an interface for reading and interpreting native Win32 resource data. 
    /// </summary>
    public interface IWin32ResourceDataReader
    {
        /// <summary>
        /// Reads and interprets the provided input stream as resource data. 
        /// </summary>
        /// <param name="dataEntry">The data entry to read and interpret the data for.</param>
        /// <param name="reader">The stream containing the raw data.</param>
        /// <returns>The interpreted win32 resource data segment.</returns>
        ISegment ReadResourceData(IResourceData dataEntry, IBinaryStreamReader reader);
    }
}