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

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides a basic implementation of a data entry that can be initialized and added to a resource
    /// directory in a PE image.
    /// </summary>
    public class ResourceData : ResourceDataBase
    {
        /// <summary>
        /// Creates a new named data entry.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        /// <param name="contents">The data to store in the entry.</param>
        public ResourceData(string name, IReadableSegment contents)
        {
            Name = name;
            Contents = contents;
        }

        /// <summary>
        /// Creates a new data entry defined by a numerical identifier..
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="contents">The data to store in the entry.</param>
        public ResourceData(uint id, IReadableSegment contents)
        {
            Contents = contents;
            Id = id;
        }

        /// <inheritdoc />
        protected override IReadableSegment GetContents()
        {
            return null;
        }
        
    }
}