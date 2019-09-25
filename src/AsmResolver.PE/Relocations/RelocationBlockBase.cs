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

namespace AsmResolver.PE.Relocations
{
    /// <summary>
    /// When overriden from this class, represents one block of relocations to be applied when the PE is loaded into memory.
    /// </summary>
    public abstract class RelocationBlockBase
    {
        private IList<RelocationEntry> _entries;

        /// <summary>
        /// Gets or sets the RVA of the page to relocate.
        /// </summary>
        public uint PageRva
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of relocations that need to be applied in this block.
        /// </summary>
        public IList<RelocationEntry> Entries
        {
            get
            {
                if (_entries is null)
                    Interlocked.CompareExchange(ref _entries, GetEntries(), null);
                return _entries;
            }
        }

        /// <summary>
        /// Obtains the relocations that need to be applied.
        /// </summary>
        /// <returns>The relocations.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Entries"/> property.</remarks>
        protected abstract IList<RelocationEntry> GetEntries();
    }
}