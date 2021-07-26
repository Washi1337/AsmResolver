using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Strings;

namespace AsmResolver.DotNet.Builder.Metadata.Strings
{
    /// <summary>
    /// Provides a mutable buffer for building up a strings stream in a .NET portable executable.
    /// </summary>
    public class StringsStreamBuffer : IMetadataStreamBuffer
    {
        private readonly MemoryStream _rawStream = new();
        private readonly IBinaryStreamWriter _writer;
        private readonly Dictionary<string, uint> _strings = new();

        /// <summary>
        /// Creates a new strings stream buffer with the default strings stream name.
        /// </summary>
        public StringsStreamBuffer()
            : this(StringsStream.DefaultName)
        {
        }

        /// <summary>
        /// Creates a new strings stream buffer.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        public StringsStreamBuffer(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _writer = new BinaryStreamWriter(_rawStream);
            _writer.WriteByte(0);
        }

        /// <inheritdoc />
        public string Name
        {
            get;
        }

        /// <inheritdoc />
        public bool IsEmpty => _rawStream.Length <= 1;

        /// <summary>
        /// Imports the contents of a strings stream and indexes all present strings.
        /// </summary>
        /// <param name="stream">The stream to import.</param>
        public void ImportStream(StringsStream stream)
        {
            uint index = 1;
            while (index < stream.GetPhysicalSize())
            {
                string @string = stream.GetStringByIndex(index)!;
                uint newIndex = AppendString(@string);
                _strings[@string] = newIndex;

                index += (uint) Encoding.UTF8.GetByteCount(@string) + 1;
            }
        }

        /// <summary>
        /// Appends raw data to the stream.
        /// </summary>
        /// <param name="data">The data to append.</param>
        /// <returns>The index to the start of the data.</returns>
        /// <remarks>
        /// This method does not index the string. Calling <see cref="AppendRawData"/> or <see cref="GetStringIndex" />
        /// on the same data will append the data a second time.
        /// </remarks>
        public uint AppendRawData(byte[] data)
        {
            uint offset = (uint) _rawStream.Length;
            _writer.WriteBytes(data, 0, data.Length);
            return offset;
        }

        private uint AppendString(string value)
        {
            uint offset = (uint) _rawStream.Length;
            AppendRawData(Encoding.UTF8.GetBytes(value));
            _writer.WriteByte(0);
            return offset;
        }

        /// <summary>
        /// Gets the index to the provided string. If the string is not present in the buffer, it will be appended to
        /// the end of the stream.
        /// </summary>
        /// <param name="value">The string to lookup or add.</param>
        /// <returns>The index of the string.</returns>
        public uint GetStringIndex(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (value!.IndexOf('\0') >= 0)
                throw new ArgumentException("String contains a zero byte.");

            if (!_strings.TryGetValue(value, out uint offset))
            {
                offset = AppendString(value);
                _strings.Add(value, offset);
            }

            return offset;
        }

        /// <summary>
        /// Serializes the strings stream buffer to a metadata stream.
        /// </summary>
        /// <returns>The metadata stream.</returns>
        public StringsStream CreateStream()
        {
            _writer.Align(4);
            return new SerializedStringsStream(Name, _rawStream.ToArray());
        }

        /// <inheritdoc />
        IMetadataStream IMetadataStreamBuffer.CreateStream() => CreateStream();
    }
}
