namespace AsmResolver.Net.Metadata
{
    /// <summary>
    /// Provides members for reading a file segment representing a method body in a .NET image.
    /// </summary>
    public interface IRawMethodBodyReader
    {
        /// <summary>
        /// Reads a raw method body segment starting at the provided offset.
        /// </summary>
        /// <param name="row">The row representing the method to read the method for.</param>
        /// <param name="reader">The reader to use for reading data from the image. This reader is positioned at the very beginning of the method body.</param>
        /// <returns>The file segment containing the method body.</returns>
        FileSegment ReadMethodBody(
            MetadataRow<FileSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint> row,
            IBinaryStreamReader reader);
    }
}