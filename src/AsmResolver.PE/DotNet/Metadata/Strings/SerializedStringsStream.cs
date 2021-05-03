using System;
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
        private readonly IDictionary<uint, string> _cachedStrings = new Dictionary<uint, string>();
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Creates a new strings stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedStringsStream(string name, byte[] rawData)
            : this(name, ByteArrayInputFile.CreateReader(rawData))
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
        public override string GetStringByIndex(uint index)
        {
            if (index == 0)
                return null;

            if (!_cachedStrings.TryGetValue(index, out string value) && index < _reader.Length)
            {
                var stringsReader = _reader.ForkRelative(index);
                var data = stringsReader.ReadBytesUntil(0);
                value = Encoding.UTF8.GetString(data, 0, data.Length - 1);
                _cachedStrings[index] = value;
            }

            return value;
        }

    }
}
