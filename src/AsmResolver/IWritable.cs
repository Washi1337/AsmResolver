using AsmResolver.IO;

namespace AsmResolver
{
    /// <summary>
    /// Represents a structure that can be serialized to an output stream.
    /// </summary>
    public interface IWritable
    {
        /// <summary>
        /// Computes the number of bytes that the structure contains.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        uint GetPhysicalSize();

        /// <summary>
        /// Serializes the structure to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write the data to.</param>
        void Write(BinaryStreamWriter writer);
    }
}
