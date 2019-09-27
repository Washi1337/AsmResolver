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
    /// Provides a basic implementation of a base relocation block, which can be instantiated and added to a PE.
    /// </summary>
    public class RelocationBlock : IRelocationBlock
    {
        private IList<RelocationEntry> _entries;

        protected RelocationBlock()
        {
        }

        /// <summary>
        /// Creates a new base relocation block for the provided page.
        /// </summary>
        /// <param name="pageRva">The virtual address of the page to apply base relocations on.</param>
        public RelocationBlock(uint pageRva)
        {
            PageRva = pageRva;
        }
        
        /// <inheritdoc />
        public uint PageRva
        {
            get;
            set;
        }

        /// <inheritdoc />
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
        protected virtual IList<RelocationEntry> GetEntries()
        {
            return new List<RelocationEntry>();
        }
        
    }
}