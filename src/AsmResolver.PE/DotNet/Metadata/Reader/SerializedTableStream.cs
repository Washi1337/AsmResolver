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

namespace AsmResolver.PE.DotNet.Metadata.Reader
{
    public class SerializedTableStream : TablesStream
    {
        private readonly IReadableSegment _contents;
        private readonly ulong _validMask;
        private readonly ulong _sortedMask;

        public SerializedTableStream(byte[] rawData)
            : this(new DataSegment(rawData))
        {
        }

        public SerializedTableStream(IReadableSegment contents)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));

            var reader = contents.CreateReader();
            Reserved = reader.ReadUInt32();
            MajorVersion = reader.ReadByte();
            MinorVersion = reader.ReadByte();
            Flags = (TablesStreamFlags) reader.ReadByte();
            Log2LargestRid = reader.ReadByte();
            _validMask = reader.ReadUInt64();
            _sortedMask = reader.ReadUInt64();
        }

        public override bool CanRead => true;

        public override IBinaryStreamReader CreateReader()
        {
            return _contents.CreateReader();
        }
        
    }
}