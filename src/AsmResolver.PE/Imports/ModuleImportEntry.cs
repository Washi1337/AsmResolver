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
using System.Threading;
using AsmResolver.PE.File;
using AsmResolver.PE.Imports.Reader;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides an implementation of the <see cref="IModuleImportEntry"/> class, which can be instantiated and added
    /// to an existing portable executable image.
    /// </summary>
    public class ModuleImportEntry : IModuleImportEntry
    {
        public static IModuleImportEntry FromReader(PEFile peFile, IBinaryStreamReader reader)
        {
            var entry = new SerializedModuleImportEntry(peFile, reader);
            return entry.IsEmpty ? null : entry;
        }
        
        private IList<MemberImportEntry> _members;

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint ForwarderChain
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IList<MemberImportEntry> Members
        {
            get
            {
                if (_members is null) 
                    Interlocked.CompareExchange(ref _members, GetMembers(), null);
                return _members;
            }
        }

        /// <summary>
        /// Obtains the collection of members that were imported.
        /// </summary>
        /// <remarks>
        /// This method is called to initialize the value of <see cref="Members" /> property.
        /// </remarks>
        /// <returns>The members list.</returns>
        protected virtual IList<MemberImportEntry> GetMembers()
        {
            return new List<MemberImportEntry>();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} ({Members.Count} members)";
        }
        
    }
}