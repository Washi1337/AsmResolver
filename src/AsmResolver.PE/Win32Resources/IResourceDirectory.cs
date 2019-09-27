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

using System.Collections.Generic;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents a single directory containing Win32 resources of a PE image. 
    /// </summary>
    public interface IResourceDirectory : IResourceDirectoryEntry
    {
        /// <summary>
        /// Gets or sets the flags of the directory.
        /// </summary>
        /// <remarks>
        /// This field is reserved and is usually set to zero. 
        /// </remarks>
        uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time that the resource data was created by the compiler.
        /// </summary>
        uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version number of the directory.
        /// </summary>
        ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version number of the directory.
        /// </summary>
        ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of entries that are stored in the directory.
        /// </summary>
        IList<IResourceDirectoryEntry> Entries
        {
            get;
        }
    }
}