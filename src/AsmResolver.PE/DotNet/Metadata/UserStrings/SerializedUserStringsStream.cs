// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

namespace AsmResolver.PE.DotNet.Metadata.UserStrings
{
    /// <summary>
    /// Provides an implementation of a user-strings stream that obtains strings from a readable segment in a file.  
    /// </summary>
    public class SerializedUserStringsStream : UserStringsStream
    {
        private readonly IDictionary<uint, string> _cachedStrings = new Dictionary<uint, string>();
        private readonly IReadableSegment _contents;

        /// <summary>
        /// Creates a new user-strings stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedUserStringsStream(string name, byte[] rawData)
            : this(name, new DataSegment(rawData))
        {
        }

        /// <summary>
        /// Creates a new user-strings stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="contents">The raw contents of the stream.</param>
        public SerializedUserStringsStream(string name, IReadableSegment contents)
            : base(name)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override IBinaryStreamReader CreateReader() => _contents.CreateReader();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _contents.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _contents.Write(writer);
        
        /// <inheritdoc />
        public override string GetStringByIndex(uint index)
        {
            if (!_cachedStrings.TryGetValue(index, out string value) && index < _contents.GetPhysicalSize())
            {
                var stringsReader = _contents.CreateReader((uint) (_contents.FileOffset + index));
                
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