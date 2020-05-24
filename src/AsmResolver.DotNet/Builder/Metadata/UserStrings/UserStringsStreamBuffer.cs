using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

namespace AsmResolver.DotNet.Builder.Metadata.UserStrings
{
    /// <summary>
    /// Provides a mutable buffer for building up a user-strings stream in a .NET portable executable. 
    /// </summary>
    public class UserStringsStreamBuffer : IMetadataStreamBuffer
    {
        private readonly MemoryStream _rawStream = new MemoryStream();
        private readonly BinaryStreamWriter _writer;
        private readonly IDictionary<string, uint> _strings = new Dictionary<string, uint>();

        /// <summary>
        /// Creates a new user-strings stream buffer with the default user-strings stream name.
        /// </summary>
        public UserStringsStreamBuffer()
            : this(UserStringsStream.DefaultName)
        {
        }

        /// <summary>
        /// Creates a new user-strings stream buffer.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        public UserStringsStreamBuffer(string name)
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
        
        /// <summary>
        /// Gets the index to the provided user-string. If the string is not present in the buffer, it will be appended to
        /// the end of the stream.
        /// </summary>
        /// <param name="value">The user-string to lookup or add.</param>
        /// <returns>The index of the user-string.</returns>
        public uint GetStringIndex(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (!_strings.TryGetValue(value, out uint offset))
            {
                offset = (uint) _rawStream.Length;

                int byteCount = Encoding.Unicode.GetByteCount(value) + 1;
                _writer.WriteCompressedUInt32((uint) byteCount);
                
                var rawData = new byte[byteCount];
                Encoding.Unicode.GetBytes(value, 0, value.Length, rawData, 0);
                rawData[byteCount - 1] = GetTerminatorByte(value);
                
                AppendRawData(rawData);
                _strings.Add(value, offset);
            }
            
            return offset;
        }

        private static byte GetTerminatorByte(string data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                char c = data[i];

                if (c >= 0x01 && c <= 0x08
                    || c >= 0x0E && c <= 0x1F
                    || c == 0x27
                    || c == 0x2D
                    || c == 0x7F
                    || c >= 0x100)
                {
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Serializes 
        /// </summary>
        /// <returns></returns>
        public UserStringsStream CreateStream()
        {
            _writer.Align(4);
            return new SerializedUserStringsStream(Name, _rawStream.ToArray());
        }

        /// <inheritdoc />
        IMetadataStream IMetadataStreamBuffer.CreateStream() => CreateStream();
    }
}