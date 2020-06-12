namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides a default implementation for the <see cref="IWin32ResourceDataReader"/>, which reads Win32 resources
    /// as raw data segments, and leaves the interpretation to the user.
    /// </summary>
    public class DefaultWin32ResourceDataReader : IWin32ResourceDataReader
    {
        /// <inheritdoc />
        public ISegment ReadResourceData(IResourceData dataEntry, IBinaryStreamReader reader) =>
            DataSegment.FromReader(reader);
    }
}