using System.Collections.Generic;
using System.IO;
using AsmResolver.PE.DotNet.Resources;

namespace AsmResolver.DotNet.Builder.Resources
{
    /// <summary>
    /// Provides a mutable buffer for building up a .NET resources directory, containing all data of all resources
    /// stored in a .NET module.
    /// </summary>
    public class DotNetResourcesDirectoryBuffer
    {
        private readonly MemoryStream _rawStream = new MemoryStream();
        private readonly BinaryStreamWriter _writer;
        private readonly IDictionary<byte[], uint> _dataOffsets = new Dictionary<byte[], uint>(ByteArrayEqualityComparer.Instance);

        /// <summary>
        /// Creates a new instance of the <see cref="DotNetResourcesDirectoryBuffer"/> class.
        /// </summary>
        public DotNetResourcesDirectoryBuffer()
        {
            _writer = new BinaryStreamWriter(_rawStream);
        }

        /// <summary>
        /// Gets the size of the buffer.
        /// </summary>
        public uint Size => (uint) _rawStream.Length;

        /// <summary>
        /// Appends raw data to the stream.
        /// </summary>
        /// <param name="data">The data to append.</param>
        /// <returns>The index to the start of the data.</returns>
        /// <remarks>
        /// This method does not index the resource data. Calling <see cref="AppendRawData"/> or <see cref="GetResourceDataOffset(byte[])"/>
        /// on the same data will append the data a second time.
        /// </remarks>
        public uint AppendRawData(byte[] data)
        {
            uint offset = (uint) _rawStream.Length;
            _writer.WriteBytes(data, 0, data.Length);
            return offset;
        }
        
        /// <summary>
        /// Gets the index to the provided resource data. If the blob is not present in the buffer, it will be appended
        /// to the end of the stream.
        /// </summary>
        /// <param name="data">The resource data to lookup or add.</param>
        /// <returns>The offset of the resource data.</returns>
        public uint GetResourceDataOffset(byte[] data)
        {
            if (data is null || data.Length == 0)
                return 0;

            if (!_dataOffsets.TryGetValue(data, out uint offset))
            {
                offset = (uint) _rawStream.Length;
                _writer.WriteUInt32((uint) data.Length);
                AppendRawData(data);
                _dataOffsets.Add(data, offset);
            }

            return offset;
        }

        /// <summary>
        /// Serialises the .NET resources directory buffer to a data directory.
        /// </summary>
        /// <returns>The metadata stream.</returns>
        public DotNetResourcesDirectory CreateDirectory() => 
            new SerializedDotNetResourcesDirectory(new DataSegment(_rawStream.ToArray()));
    }
}