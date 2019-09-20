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

namespace AsmResolver
{
    /// <summary>
    /// Provides members for converting virtual addresses to file offsets and vice versa. 
    /// </summary>
    public interface IOffsetConverter
    {
        /// <summary>
        /// Converts a file offset to the virtual address when it is loaded into memory.
        /// </summary>
        /// <param name="fileOffset">The file offset to convert.</param>
        /// <returns>The virtual address, relative to the image base.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the file offset falls outside of the range of the convertible file offsets.</exception>
        uint FileOffsetToRva(uint fileOffset);
        
        /// <summary>
        /// Converts a virtual address to the physical file offset.
        /// </summary>
        /// <param name="rva">The virtual address, relative to the image base, to convert.</param>
        /// <returns>The file offset.</returns>
        /// /// <exception cref="ArgumentOutOfRangeException">Occurs when the virtual address falls outside of the range of the convertible addresses.</exception>
        uint RvaToFileOffset(uint rva);
    }
}