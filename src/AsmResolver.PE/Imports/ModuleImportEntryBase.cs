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

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// When overriden from this class, represents a single module that was imported into a portable executable
    /// as part of the imports data directory. Each instance represents one entry in the imports directory.
    /// </summary>
    public abstract class ModuleImportEntryBase
    {
        /// <summary>
        /// Gets or sets the name of the module that was imported.
        /// </summary>
        public abstract string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time stamp that the module was loaded into memory.
        /// </summary>
        /// <remarks>
        /// This field is always 0 if the PE was read from the disk.
        /// </remarks>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the first member that is a forwarder.
        /// </summary>
        public uint ForwarderChain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of members from the module that were imported.
        /// </summary>
        public abstract IList<MemberImportEntry> Members
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} ({Members.Count} members)";
        }
        
    }
}