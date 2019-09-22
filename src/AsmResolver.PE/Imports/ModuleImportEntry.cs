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
    /// Provides an implementation of the <see cref="ModuleImportEntryBase"/> class, which can be instantiated and added
    /// to an existing portable executable image.
    /// </summary>
    public class ModuleImportEntry : ModuleImportEntryBase
    {
        /// <summary>
        /// Creates a new module import.
        /// </summary>
        /// <param name="name">The name of the module to import.</param>
        public ModuleImportEntry(string name)
        {
            Name = name;
            Members = new List<MemberImportEntry>();
        }

        /// <inheritdoc />
        public override string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override IList<MemberImportEntry> Members
        {
            get;
        }
    }
}