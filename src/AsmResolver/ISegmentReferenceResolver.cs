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
    /// Provides members for resolving virtual addresses to a segment in a binary file.
    /// </summary>
    public interface ISegmentReferenceResolver
    {
        /// <summary>
        /// Resolves the provided virtual address to a segment reference.  
        /// </summary>
        /// <param name="rva">The virtual address of the segment.</param>
        /// <returns>The reference to the segment.</returns>
        ISegmentReference GetReferenceToRva(uint rva);
    }
}