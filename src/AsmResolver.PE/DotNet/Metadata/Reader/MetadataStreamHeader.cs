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
    public readonly struct MetadataStreamHeader
    {
        public static MetadataStreamHeader FromReader(IBinaryStreamReader reader)
        {
            uint offset = reader.ReadUInt32();
            uint size = reader.ReadUInt32();
            string name = reader.ReadAsciiString();
            reader.Align(4);
            return new MetadataStreamHeader(offset, size, name);
        }
        
        public MetadataStreamHeader(uint offset, uint size, string name)
        {
            Offset = offset;
            Size = size;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            
            if (name.Length > 32)
                throw new ArgumentOutOfRangeException(nameof(name), "Name can be no longer than 32 bytes.");
        }
        
        public uint Offset
        {
            get;
        }

        public uint Size
        {
            get;
        }

        public string Name
        {
            get;
        }

        public override string ToString()
        {
            return $"{nameof(Offset)}: {Offset:X8}, {nameof(Size)}: {Size:X8}, {nameof(Name)}: {Name}";
        }
    }
}