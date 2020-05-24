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
    /// Represents a single data entry in a Win32 resource directory.
    /// </summary>
    public interface IResourceData : IResourceEntry
    {
        /// <summary>
        /// Gets or sets the raw contents of the data entry.
        /// </summary>
        IReadableSegment Contents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the code page that is used to decode code point values within the resource data. 
        /// </summary>
        /// <remarks>
        /// Typically, the code page would be the Unicode code page.
        /// </remarks>
        uint CodePage
        {
            get;
            set;
        }
    }
}