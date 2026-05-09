using System.Collections.Concurrent;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides an implementation of a strings stream that obtains strings from a readable segment in a file.
    /// </summary>
    public class SerializedStringsStream : StringsStream
    {
        private readonly ConcurrentDictionary<uint, Utf8String> _cachedStrings = new();
        private readonly BinaryStreamReaderState _readerState;

        /// <summary>
        /// Creates a new strings stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedStringsStream(byte[] rawData)
            : this(DefaultName, new BinaryStreamReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new strings stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedStringsStream(string name, byte[] rawData)
            : this(name, new BinaryStreamReader(rawData))
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
            _readerState = reader.GetState();
            Offset = reader.Offset;
            Rva = reader.Rva;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _readerState.CreateReader();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _readerState.Length;

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer) => _readerState.CreateReader().WriteToOutput(writer);

        /// <inheritdoc />
        public override Utf8String? GetStringByIndex(uint index)
        {
            if (index == 0)
                return null;

            if (!_cachedStrings.TryGetValue(index, out var value) && index < _readerState.Length)
            {
                var stringsReader = _readerState.WithRelativeOffset(index).CreateReader();
                value = stringsReader.ReadUtf8String();
                _cachedStrings.TryAdd(index, value);
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

            var reader = _readerState.CreateReader();
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

        /// <inheritdoc />
        public override IEnumerable<(uint Index, Utf8String String)> EnumerateStrings()
        {
            uint currentIndex = 1;

            while (currentIndex < _readerState.Length)
            {
                var result = GetStringByIndex(currentIndex);

                if (result is null)
                    break;

                yield return (currentIndex, result);

                currentIndex += (uint) result.ByteCount;
                currentIndex++; // Zero terminator.
            }
        }
    }
}
