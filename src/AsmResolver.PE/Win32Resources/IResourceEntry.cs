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

using AsmResolver.Collections;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents one entry in a win32 resource directory.
    /// </summary>
    public interface IResourceEntry : IOwnedCollectionElement<IResourceDirectory>
    {
        /// <summary>
        /// Gets the parent directory the entry is stored in.
        /// </summary>
        IResourceDirectory ParentDirectory
        {
            get;
        }
        
        /// <summary>
        /// Gets or sets the name of the entry.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID of the entry.
        /// </summary>
        uint Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating the entry is a sub directory or not.
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// Gets a value indicating the entry is a data entry or not.
        /// </summary>
        bool IsData { get; }
    }
}