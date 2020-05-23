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

namespace AsmResolver
{
    /// <summary>
    /// Represents a single chunk of data residing in a file or memory space.
    /// </summary>
    public interface ISegment : IOffsetProvider, IWritable
    {
        /// <summary>
        /// Computes the number of bytes the segment will contain when it is mapped into memory.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        uint GetVirtualSize();
        
    }

    public static partial class Extensions
    {
        public static uint Align(this uint value, uint alignment)
        {
            alignment--;
            return (value + alignment) & ~alignment;
        }

        public static uint GetCompressedSize(this uint value)
        {
            if (value < 0x80)
                return sizeof(byte);
            if (value < 0x4000)
                return sizeof(ushort);
            return sizeof(uint);
        }
    }
}