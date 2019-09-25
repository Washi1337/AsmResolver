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

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides a basic implementation of a resource directory that can be initialized and added to another resource
    /// directory or used as a root resource directory of a PE image.
    /// </summary>
    public class ResourceDirectory : ResourceDirectoryBase
    {
        /// <summary>
        /// Creates a new named resource directory. 
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        public ResourceDirectory(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        /// <summary>
        /// Creates a new resource directory defined by a numeric identifier. 
        /// </summary>
        /// <param name="id">The identifier.</param>
        public ResourceDirectory(uint id)
        {
            Id = id;
        }

        /// <inheritdoc />
        protected override IList<IResourceDirectoryEntry> GetEntries()
        {
            return new List<IResourceDirectoryEntry>();
        }
        
    }
}