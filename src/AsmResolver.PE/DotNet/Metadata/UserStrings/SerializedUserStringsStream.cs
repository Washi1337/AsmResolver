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
    public class SerializedUserStringsStream : UserStringsStream
    {
        private readonly IDictionary<int, string> _cachedStrings = new Dictionary<int, string>();
        private readonly IReadableSegment _contents;

        public SerializedUserStringsStream(byte[] rawData)
            : this(new DataSegment(rawData))
        {
        }

        public SerializedUserStringsStream(IReadableSegment contents)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override IBinaryStreamReader CreateReader()
        {
            return _contents.CreateReader();
        }

        /// <inheritdoc />
        public override string GetStringByIndex(int index)
        {
            if (!_cachedStrings.TryGetValue(index, out string value) && index < _contents.GetPhysicalSize())
            {
                var stringsReader = _contents.CreateReader((uint) (_contents.FileOffset + index));
                
                // Try read length.
                if (stringsReader.TryReadCompressedUInt32(out uint length))
                {
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