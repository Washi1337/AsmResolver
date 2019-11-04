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

namespace AsmResolver.PE.DotNet.Metadata.Strings
{
    /// <summary>
    /// Provides an implementation of a strings stream that obtains strings from a readable segment in a file.  
    /// </summary>
    public class SerializedStringsStream : StringsStream
    {
        private readonly IDictionary<uint, string> _cachedStrings = new Dictionary<uint, string>();
        private readonly IReadableSegment _contents;

        /// <summary>
        /// Creates a new strings stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedStringsStream(string name, byte[] rawData)
            : this(name, new DataSegment(rawData))
        {
        }

        /// <summary>
        /// Creates a new strings stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="contents">The raw contents of the stream.</param>
        public SerializedStringsStream(string name, IReadableSegment contents)
            : base(name)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override IBinaryStreamReader CreateReader() => _contents.CreateReader();

        public override uint GetPhysicalSize() => _contents.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _contents.Write(writer);

        /// <inheritdoc />
        public override string GetStringByIndex(uint index)
        {
            if (index == 0)
                return null;
            
            if (!_cachedStrings.TryGetValue(index, out string value) && index < _contents.GetPhysicalSize())
            {
                var stringsReader = _contents.CreateReader((uint) (_contents.FileOffset + index));
                var data = stringsReader.ReadBytesUntil(0);
                value = Encoding.UTF8.GetString(data, 0, data.Length - 1);
                _cachedStrings[index] = value;
            }

            return value;
        }

    }
}