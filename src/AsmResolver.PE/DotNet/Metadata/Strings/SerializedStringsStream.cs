using System.Collections.Generic;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Strings
{
    /// <summary>
    /// Provides an implementation of a strings stream that obtains strings from a readable segment in a file.
    /// </summary>
    public class SerializedStringsStream : StringsStream
    {
        private readonly Dictionary<uint, Utf8String> _cachedStrings = new();
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Creates a new strings stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedStringsStream(string name, byte[] rawData)
            : this(name, ByteArrayDataSource.CreateReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new strings stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="reader">The raw contents of the stream.</param>
        public SerializedStringsStream(string name, in BinaryStreamReader reader)
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
        public override Utf8String? GetStringByIndex(uint index)
        {
            if (index == 0)
                return null;

            if (!_cachedStrings.TryGetValue(index, out var value) && index < _reader.Length)
            {
                var stringsReader = _reader.ForkRelative(index);
                byte[] rawData = stringsReader.ReadBytesUntil(0);

                if (rawData.Length == 0)
                {
                    value = Utf8String.Empty;
                }
                else
                {
                    // Trim off null terminator byte if its present.
                    int actualLength = rawData.Length;
                    if (rawData[actualLength - 1] == 0)
                        actualLength--;

                    value = new Utf8String(rawData, 0, actualLength);
                }

                _cachedStrings[index] = value;
            }

            return value;
        }

    }
}
