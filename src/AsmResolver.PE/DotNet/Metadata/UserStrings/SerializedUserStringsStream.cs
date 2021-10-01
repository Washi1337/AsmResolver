using System.Collections.Generic;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.UserStrings
{
    /// <summary>
    /// Provides an implementation of a user-strings stream that obtains strings from a readable segment in a file.
    /// </summary>
    public class SerializedUserStringsStream : UserStringsStream
    {
        private readonly Dictionary<uint, string> _cachedStrings = new();
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Creates a new user-strings stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedUserStringsStream(string name, byte[] rawData)
            : this(name, ByteArrayDataSource.CreateReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new user-strings stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="reader">The raw contents of the stream.</param>
        public SerializedUserStringsStream(string name, BinaryStreamReader reader)
            : base(name)
        {
            _reader = reader;
            Offset = reader.Offset;
            Rva = reader.Rva;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _reader.Fork();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _reader.Length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _reader.Fork().WriteToOutput(writer);

        /// <inheritdoc />
        public override string? GetStringByIndex(uint index)
        {
            if (!_cachedStrings.TryGetValue(index, out string value) && index < _reader.Length)
            {
                var stringsReader = _reader.ForkRelative(index);

                // Try read length.
                if (stringsReader.TryReadCompressedUInt32(out uint length))
                {
                    if (length == 0)
                        return string.Empty;

                    // Read unicode bytes.
                    var data = new byte[length];
                    int actualLength = stringsReader.ReadBytes(data, 0, (int) length);

                    // Exclude the terminator byte.
                    value = Encoding.Unicode.GetString(data, 0, actualLength - 1);
                }

                _cachedStrings[index] = value;
            }

            return value;
        }
    }
}
