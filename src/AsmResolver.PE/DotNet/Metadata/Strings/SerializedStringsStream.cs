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
        /// Creates a new strings stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedStringsStream(byte[] rawData)
            : this(DefaultName, ByteArrayDataSource.CreateReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new strings stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedStringsStream(string name, byte[] rawData)
            : this(name, ByteArrayDataSource.CreateReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new strings stream with the provided file segment reader as the raw contents of the stream.
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
                byte[] rawData = stringsReader.ReadBytesUntil(0, false);

                value = rawData.Length != 0
                    ? new Utf8String(rawData)
                    : Utf8String.Empty;

                _cachedStrings[index] = value;
            }

            return value;
        }

        /// <inheritdoc />
        public override bool TryFindStringIndex(Utf8String? value, out uint index)
        {
            if (value is null)
            {
                index = 0;
                return true;
            }

            byte[] bytes = value.GetBytesUnsafe();

            var reader = _reader.Fork();
            while (reader.CanRead((uint) value.ByteCount))
            {
                index = reader.RelativeOffset;

                int i = 0;
                for (; i < bytes.Length; i++)
                {
                    if (bytes[i] != reader.ReadByte())
                        break;
                }

                if (i == value.Length && reader.CanRead(sizeof(byte)) && reader.ReadByte() == 0)
                    return true;

                reader.RelativeOffset = index + 1;
            }

            index = 0;
            return false;
        }
    }
}
